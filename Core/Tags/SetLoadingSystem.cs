using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Hjson;
using Newtonsoft.Json.Linq;
using ReLogic.Reflection;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Tags;

//TODO: Optimize by writing a memory-efficient HJSON parser?
internal sealed partial class SetLoadingSystem : ModSystem
{
	private struct StorageInfo
	{
		public SetStorage Handle;
		public IdDictionary Search;
		public Dictionary<string, bool[]> Sets;

		public StorageInfo(SetStorage handle, IdDictionary search, Type? setsType)
		{
			Handle = handle;
			Search = search;
			Sets = new();

			if (setsType != null) {
				foreach (var field in setsType.GetFields(BindingFlags.Static | BindingFlags.Public)) {
					if (field.FieldType != typeof(bool[])) continue;
					if (field.GetValue(null) is bool[] set) Sets[field.Name] = set;
				}
			}
		}
	}

	private static Dictionary<string, StorageInfo> storageMappings = null!;
	private static readonly string extension = ".tags.hjson";

	public override void Load()
	{
		storageMappings = new() {
			{ "NPCID", new(ContentSets.GetStorageHandle<NPCID>(), NPCID.Search, typeof(NPCID.Sets)) },
			{ "ItemID", new(ContentSets.GetStorageHandle<ItemID>(), ItemID.Search, typeof(ItemID.Sets)) },
			{ "TileID", new(ContentSets.GetStorageHandle<TileID>(), TileID.Search, typeof(TileID.Sets)) },
			{ "WallID", new(ContentSets.GetStorageHandle<WallID>(), WallID.Search, typeof(WallID.Sets)) },
			{ "GoreID", new(ContentSets.GetStorageHandle<GoreID>(), GoreID.Search, typeof(GoreID.Sets)) },
			{ "ProjectileID", new(ContentSets.GetStorageHandle<ProjectileID>(), ProjectileID.Search, typeof(ProjectileID.Sets)) },
			{ "NPCAIStyleID", new(ContentSets.GetStorageHandle<NPCAIStyleID>(), NPCAIStyleID.Search, null) },
			{ "ProjAIStyleID", new(ContentSets.GetStorageHandle<ProjAIStyleID>(), ProjAIStyleID.Search, null) },
		};

		LoadDataFromMod(Mod);
	}

	public override void OnModLoad()
		=> ContentSets.Recalculate();

	public override void PostSetupContent()
		=> ContentSets.Recalculate();

	public static void LoadDataFromMod(Mod mod)
	{
		var assets = mod.GetFileNames();
		List<Exception>? errors = null;

		foreach (string fullFilePath in assets.Where(t => t.EndsWith(extension))) {
			using var stream = mod.GetFileStream(fullFilePath);
			using var streamReader = new StreamReader(stream);
			string hjsonText = streamReader.ReadToEnd();

			try {
				string jsonText = HjsonValue.Parse(hjsonText).ToString(Stringify.Plain);
				var json = JObject.Parse(jsonText);

				foreach (var rootPair in json) {
					if (rootPair is not { Key: string setName, Value: JObject setJson })
						continue;

					ContentSet set = ContentSet.GetOrCreate(setName);

					foreach (var property in setJson.Properties()) {
						if (property.Value is not JArray jsonArray || operationKeyRegex.Match(property.Name) is not { Success: true } match)
							continue;

						string[] values = jsonArray.Values<string>().ToArray()!;
						string[] contexts = match.Groups[2].Value.Split('|');

						try {
							switch (match.Groups[1].Value) {
								case "Sets": IncludeSets(set, values); break;
								case "Includes": IncludeValues(set, contexts, values); break;
								case "LegacySets": IncludeLegacySets(set, contexts, values); break;
								case "Excludes": throw new NotImplementedException();
							}
						}
						catch (Exception e) {
							(errors ??= new()).Add(e);
						}
					}
				}
			}
			catch (Exception e) {
				(errors ??= new()).Add(e);
			}
		}

		if (errors != null)
			throw new AggregateException($"Errors occurred parsing *{extension} files", errors);
	}

	[GeneratedRegex(@"(\w+)(?:\.((?:\w+\|?)*))?", RegexOptions.Compiled)]
	private static partial Regex OperationKeyRegex();
	private static readonly Regex operationKeyRegex = OperationKeyRegex();

	private static StorageInfo GetStorageByName(string name)
	{
		return storageMappings.TryGetValue(name, out var result) ? result : throw new ArgumentException($"Unknown set storage: '{name}'.");
	}

	private static void IncludeSets(ContentSet set, ReadOnlySpan<string> setNames)
	{
		Span<ContentSet> sets = stackalloc ContentSet[setNames.Length];
		for (int i = 0; i < sets.Length; i++)
			sets[i] = ContentSet.GetOrCreate(setNames[i]); // TODO: Restructure processing order and use Get here?

		ContentSets.IncludeSets(set, sets);
	}
	private static void IncludeLegacySets(ContentSet set, ReadOnlySpan<string> contexts, ReadOnlySpan<string> setNames)
	{
		var rent = ArrayPool<bool[]>.Shared.Rent(setNames.Length);
		var arrays = rent.AsSpan(0, setNames.Length);

		try {
			foreach (string context in contexts) {
				var storage = GetStorageByName(context);

				for (int i = 0; i < arrays.Length; i++)
					arrays[i] = storage.Sets[setNames[i]];

				ContentSets.IncludeLegacySets(storage.Handle, set, arrays);
			}
		}
		finally {
			ArrayPool<bool[]>.Shared.Return(rent);
		}
	}
	private static void IncludeValues(ContentSet set, ReadOnlySpan<string> contexts, ReadOnlySpan<string> values)
	{
		Span<ushort> indices = stackalloc ushort[values.Length];

		foreach (string context in contexts) {
			var storage = GetStorageByName(context);

			for (int i = 0; i < values.Length; i++)
				indices[i] = (ushort)storage.Search.GetId(values[i]);

			ContentSets.Include(storage.Handle, set, indices);
		}
	}
}
