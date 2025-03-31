using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Hjson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.ModLoader;

// Prefabs are immutable entities loaded from data files.

namespace TerrariaOverhaul.Core.Data;

public readonly struct Prefab
{
	public static readonly Prefab Invalid = default;

	public readonly uint Index;
	public readonly uint Version;
	public readonly bool IsValid => DataStorage.IsEntityValid(ToEntity());

	public bool Has(ComponentMask mask) => DataStorage.HasComponents(ToEntity(), mask);
	public readonly bool Has<T>() where T : IComponent => DataStorage.HasComponent<T>(ToEntity());
	public readonly ref readonly T Get<T>() where T : IComponent => ref DataStorage.GetComponent<T>(ToEntity());

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private readonly DataEntity ToEntity() => Unsafe.BitCast<Prefab, DataEntity>(this);

	public static implicit operator Prefab(DataEntity entity) => Unsafe.BitCast<DataEntity, Prefab>(entity);
}

public struct PrefabInfo : IComponent
{
	public required string Identifier;

	public static implicit operator string(in PrefabInfo info) => info.Identifier;
}

public static class Prefabs
{
	private static readonly Dictionary<string, Prefab> prefabsByName = [];

	public static void RegisterJsonConverter(JsonConverter converter)
		=> PrefabLoading.RegisterJsonConverter(converter);

	public static DataEntity CreatePrefab(string identifier)
	{
		var entity = DataStorage.CreateEntity();
		entity.Add(new PrefabInfo {
			Identifier = identifier,
		});
		prefabsByName[identifier] = entity;
		return entity;
	}

	public static Prefab GetPrefab(string identifier)
		=> prefabsByName[identifier];

	public static bool TryGetPrefab(string identifier, [NotNullWhen(true)] out Prefab prefab)
		=> prefabsByName.TryGetValue(identifier, out prefab);

	public static Query Query()
	{
		return DataStorage.CreateQuery().With<PrefabInfo>();
	}
}

//TODO: Optimize by writing a memory-efficient HJSON parser?
internal sealed partial class PrefabLoading : ModSystem
{
	private static readonly string extension = ".prefab.hjson";
	private static readonly HashSet<Type> jsonConverterTypes = [];
	private static readonly JsonSerializer jsonSerializer = new();

	public static void RegisterJsonConverter(JsonConverter converter)
	{
		if (jsonConverterTypes.Add(converter.GetType()))
			jsonSerializer.Converters.Add(converter);
	}

	public override void Load()
	{
		RegisterJsonConverter(new SoundStyleJsonConverter());
		LoadDataFromMod(Mod);
	}

	public static void LoadDataFromMod(Mod mod)
	{
		DataStorage.RegisterTypesFromAssembly(mod.Code);

		var assets = mod.GetFileNames();
		List<Exception>? errors = null;

		foreach (string fullFilePath in assets.Where(t => t.EndsWith(extension))) {
			using var stream = mod.GetFileStream(fullFilePath);
			using var streamReader = new StreamReader(stream);

			string prefabName = Path.GetFileName(fullFilePath)[0..^extension.Length];
			string hjsonText = streamReader.ReadToEnd();
			var entity = Prefabs.CreatePrefab(prefabName);

			using var jsonReader = new JsonTextReader(streamReader);

			try {
				var json =  JObject.Parse(HjsonValue.Parse(hjsonText).ToString(Stringify.Plain));

				foreach (var rootPair in json) {
					if (rootPair is not { Key: string componentName, Value: JObject componentJson })
						continue;

					if (!DataStorage.TryGetComponentFromName(componentName, out var component))
						throw new KeyNotFoundException($"Unknown component: '{componentName}'.");

					var componentType = component.GetComponentType();
					var value = componentJson.ToObject(componentType, jsonSerializer);

					entity.Add(component, value!);
				}
			}
			catch (Exception e) {
				(errors ??= new()).Add(e);
			}
		}

		if (errors != null)
			throw new AggregateException($"Errors occurred parsing *{extension} files", errors);
	}
}
