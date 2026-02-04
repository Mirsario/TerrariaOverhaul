using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Hjson;
using Microsoft.NET.StringTools;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;

// Prefabs are immutable entities loaded from data files.

namespace TerrariaOverhaul.Core.Data;

internal readonly struct Prefab
{
	public static readonly Prefab Invalid = default;

	public readonly uint Index;
	public readonly uint Version;
	public readonly bool IsValid => DataStorage.IsEntityValid(ToEntity());

	public bool Has(ComponentMask mask) => DataStorage.HasAllComponents(ToEntity(), mask);
	public readonly bool Has<T>() where T : IComponent => DataStorage.HasComponent<T>(ToEntity());
	public readonly ref readonly T Get<T>() where T : IComponent => ref DataStorage.GetComponent<T>(ToEntity());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly DataEntity ToEntity() => Unsafe.BitCast<Prefab, DataEntity>(this);

	public static implicit operator Prefab(DataEntity entity) => Unsafe.BitCast<DataEntity, Prefab>(entity);
}

internal struct PrefabInfo() : IComponent
{
	public required string Identifier;
	public uint Generation = 1;

	public static implicit operator string(in PrefabInfo info) => info.Identifier;
}

internal static class Prefabs
{
	private static readonly Dictionary<string, Prefab> prefabsByName = [];

	public static void RegisterJsonConverter(JsonConverter converter)
		=> PrefabLoading.RegisterJsonConverter(converter);

	public static DataEntity Create(string identifier)
	{
		if (prefabsByName.ContainsKey(identifier))
			throw new ArgumentException($"Prefab '{identifier}' already exists!");

		var entity = New(identifier);
		prefabsByName.Add(identifier, entity);
		return entity;
	}
	public static DataEntity CreateOrReplace(string identifier)
	{
		DataEntity entity;
		if (TryGet(identifier, out var prefab)) {
			entity = ToEntity(prefab);
			var info = entity.Get<PrefabInfo>();
			entity.Clear();
			entity.Add(info with { Generation = info.Generation + 1 });
		} else {
			entity = New(identifier);
			prefabsByName[identifier] = entity;
		}

		return entity;
	}

	private static DataEntity New(string identifier)
	{
		var entity = DataStorage.CreateEntity();
		entity.Add(new PrefabInfo { Identifier = identifier });
		return entity;
	}

	public static Prefab Get(string identifier)
		=> prefabsByName[identifier];

	public static bool TryGet(string identifier, [NotNullWhen(true)] out Prefab prefab)
		=> prefabsByName.TryGetValue(identifier, out prefab);

	public static bool Remove(string identifier)
	{
		if (TryGet(identifier, out var prefab)) {
			prefabsByName.Remove(identifier);
			ToEntity(prefab).Destroy();
			return true;
		}

		return false;
	}

	public static Query Query()
	{
		return DataStorage.CreateQuery().With<PrefabInfo>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static DataEntity ToEntity(Prefab prefab) => Unsafe.BitCast<Prefab, DataEntity>(prefab);
}

//TODO: Optimize by writing a memory-efficient HJSON parser?
internal sealed partial class PrefabLoading : ModSystem
{
	private static readonly string extension = ".prefab.hjson";
	private static readonly HashSet<Type> jsonConverterTypes = [];
	private static readonly JsonSerializer jsonSerializer = new();
	private static FileSystemWatcher? watcher;

	public static void RegisterJsonConverter(JsonConverter converter)
	{
		if (jsonConverterTypes.Add(converter.GetType()))
			jsonSerializer.Converters.Add(converter);
	}

	public override void Load()
	{
		RegisterJsonConverter(new SoundStyleJsonConverter());

		ThreadUtils.RunOnMainThread(() => {
			LoadDataFromMod(Mod);
			SetupFileWatcher(Mod);
		});
	}

	public static void LoadDataFromMod(Mod mod)
	{
		DataStorage.RegisterTypesFromAssembly(mod.Code);

		var assets = mod.GetFileNames();
		List<Exception>? errors = null;

		foreach (string fullFilePath in assets.Where(t => t.EndsWith(extension))) {
			try {
				LoadFile(mod, fullFilePath);
			}
			catch (Exception e) {
				(errors ??= new()).Add(e);
			}
		}

		if (errors != null)
			throw new AggregateException($"Errors occurred parsing *{extension} files", errors);
	}

	private static void UnloadFile(string filePath)
	{
		string prefabName = Path.GetFileName(filePath)[0..^extension.Length];
		Prefabs.Remove(prefabName);
	}
	private static void LoadFile(Mod mod, string filePath, bool fileWatch = false)
	{
		using var stream = fileWatch && Path.IsPathFullyQualified(filePath) ? File.OpenRead(filePath) : mod.GetFileStream(filePath);
		using var streamReader = new StreamReader(stream);

		string prefabName = Path.GetFileName(filePath)[0..^extension.Length];
		string hjsonText = streamReader.ReadToEnd();

		DataEntity entity;

		if (!fileWatch) {
			entity = Prefabs.Create(prefabName);
		} else {
			entity = Prefabs.CreateOrReplace(prefabName);
		}

		using var jsonReader = new JsonTextReader(streamReader);

		var json = JObject.Parse(HjsonValue.Parse(hjsonText).ToString(Stringify.Plain));

		foreach (var rootPair in json) {
			if (rootPair is not { Key: string componentName, Value: JObject componentJson })
				continue;

			if (!DataStorage.TryGetComponentFromName(componentName, out var component))
				throw new KeyNotFoundException($"Unknown component: '{componentName}'.");

			var componentType = component.GetComponentType();
			var value = componentJson.ToObject(componentType, jsonSerializer);

			entity.AddByHandle(component, value!);
		}
	}

	[Conditional("DEBUG")]
	private static void SetupFileWatcher(Mod mod)
	{
		if (string.IsNullOrWhiteSpace(mod.SourceFolder) || !Directory.Exists(mod.SourceFolder))
			return;

		watcher = new FileSystemWatcher(mod.SourceFolder) {
			EnableRaisingEvents = true,
			IncludeSubdirectories = true,
			NotifyFilter = NotifyFilters.LastWrite,
		};
		watcher.Changed += OnFileUpdate;
		watcher.Created += OnFileUpdate;
		watcher.Deleted += OnFileUpdate;
		watcher.Renamed += (sender, args) => {
			if (Path.GetDirectoryName(args.FullPath) is not { Length: > 0 } oldDir) return;
			if (Path.GetDirectoryName(args.FullPath) is not { Length: > 0 } newDir) return;
			OnFileUpdate(sender, new(WatcherChangeTypes.Deleted, oldDir, args.OldName));
			OnFileUpdate(sender, new(WatcherChangeTypes.Created, newDir, args.Name));
		};

		mod.Logger.Info("Activated content filewatcher.");
	}

	private static void OnFileUpdate(object sender, FileSystemEventArgs args)
	{
		if (!args.FullPath.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase)) return;

		ThreadUtils.RunOnMainThread(() => {
			using var _ = new Logging.QuietExceptionHandle();

			if (args.ChangeType is WatcherChangeTypes.Deleted) {
				UnloadFile(args.FullPath);
				MessageUtils.NewText($"Prefab '{args.Name}' was deleted.", Color.Orange, logAsInfo: true);
				return;
			}
			
			try {
				LoadFile(OverhaulMod.Instance, args.FullPath, fileWatch: true);
				MessageUtils.NewText($"Reloaded prefab '{args.Name}'.", Color.BlueViolet, logAsInfo: true);
			}
			catch (IOException) { }
			catch (Exception err) {
				MessageUtils.NewText($"Reload failed for '{args.Name}': {err.Message}.", Color.IndianRed, logAsInfo: true);
			}
		});
	}
}