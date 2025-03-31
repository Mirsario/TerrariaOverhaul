using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using ReLogic.Reflection;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Utilities;
using BitMask64 = TerrariaOverhaul.Utilities.BitMask<ulong>;
using ValueIndex = ushort;

namespace TerrariaOverhaul.Core.Tags;

public readonly struct ContentSet
{
	public readonly int Id;

	internal ContentSet(int id)
	{
		Id = id;
	}

	public bool Has<TStorage>(int entryId)
		=> ContentSets.Has(ContentSets.GetStorageHandle<TStorage>(), this, entryId);
	
	public ReadOnlySpan<BitMask64> Values<TStorage>()
		=> ContentSets.Values(ContentSets.GetStorageHandle<TStorage>(), this);

	public ContentSet Include<TStorage>(params ValueIndex[] indices) => Include<TStorage>((ReadOnlySpan<ValueIndex>)indices);
	public ContentSet Include<TStorage>(ReadOnlySpan<ValueIndex> indices)
	{
		ContentSets.Include(ContentSets.GetStorageHandle<TStorage>(), this, indices);
		return this;
	}

	public ContentSet AttachSets(ReadOnlySpan<ContentSet> sets)
	{
		ContentSets.IncludeSets(this, sets);
		return this;
	}
	public ContentSet AttachLegacySets<TStorage>(ReadOnlySpan<bool[]> sets)
	{
		ContentSets.IncludeLegacySets(ContentSets.GetStorageHandle<TStorage>(), this, sets);
		return this;
	}

	public static bool TryGet(string identifier, out ContentSet result) => ContentSets.TryGet(identifier, out result);
	public static ContentSet Get(string identifier) => ContentSets.Get(identifier);
	public static ContentSet GetOrCreate(string identifier) => ContentSets.GetOrCreate(identifier);

	public static implicit operator ContentSet(string identifier)
		=> ContentSets.GetOrCreate(identifier);
}

public readonly record struct SetStorage(int Id);

internal static class ContentSets
{
	internal static class GenericData<T>
	{
		public static SetStorage Storage;
		static GenericData() => RuntimeHelpers.RunClassConstructor(typeof(ContentSets).TypeHandle);
	}

	private struct GlobalSetData()
	{
#if DEBUG
		public string Name = "Unknown";
#endif
		public HashSet<ContentSet>? IncludedSets = null;
	}
	private struct StorageSetData()
	{
		public BitMaskArray<ulong> Values = new();
		public HashSet<ValueIndex>? ManualInclusions = null;
		public HashSet<bool[]>? IncludedLegacySets = null;
	}
	private struct StorageData(IdDictionary search)
	{
		public StorageSetData[] Sets = [];
		public BitMaskArray<ulong> DirtySetsMasks = new();
		public IdDictionary Search = search;

		public void EnsureCapacity()
		{
			int targetLength = globalSetData.Length;
			if (Sets.Length != targetLength) {
				int oldSetsLength = Sets.Length;
				int oldMasksLength = DirtySetsMasks.Array.Length;
				Array.Resize(ref Sets, targetLength);
				Array.Resize(ref DirtySetsMasks.Array, (targetLength / BitMask64.BitSize) + 1);
				for (int i = oldSetsLength; i < Sets.Length; i++) Sets[i] = new();
				for (int i = oldMasksLength; i < DirtySetsMasks.Array.Length; i++) DirtySetsMasks.Array[i] = new();
			}
		}
	}

	private static int storageCount;
	private static StringIdMap stringIdMap = new();
	private static StorageData[] storages = new StorageData[4];
	private static GlobalSetData[] globalSetData = [];

	//private static int RegisteredSetCount => stringIdMap.NextId;

	static ContentSets()
	{
		RegisterStorage<NPCID>(NPCID.Search);
		RegisterStorage<ItemID>(ItemID.Search);
		RegisterStorage<TileID>(TileID.Search);
		RegisterStorage<WallID>(WallID.Search);
		RegisterStorage<ProjectileID>(ProjectileID.Search);
		RegisterStorage<NPCAIStyleID>(NPCAIStyleID.Search);
		RegisterStorage<ProjAIStyleID>(ProjAIStyleID.Search);
	}

	public static ContentSet Get(string identifier)
		=> new(stringIdMap.IdLookup[identifier]);

	public static bool TryGet(string identifier, out ContentSet result)
	{
		if (stringIdMap.IdLookup.TryGetValue(identifier, out int id)) {
			result = new(id);
			return true;
		}

		result = default;
		return false;
	}

	public static ContentSet GetOrCreate(string identifier)
	{
		int id = stringIdMap.IdFromString(identifier);
		if (id >= globalSetData.Length) {
			int oldLength = globalSetData.Length;
			Array.Resize(ref globalSetData, (int)BitOperations.RoundUpToPowerOf2((uint)(id + 1)));
			for (int i = oldLength; i < globalSetData.Length; i++) globalSetData[i] = new();
		}

#if DEBUG
		globalSetData[id].Name = identifier;
#endif

		return new(id);
	}

	private static ref StorageData GetStorageData(SetStorage storage) => ref storages[storage.Id - 1];

	public static SetStorage GetStorageHandle<T>()
		=> GenericData<T>.Storage is { Id: not 0 } storage ? storage : throw new InvalidOperationException($"'{typeof(T).Name}' is not a registered set storage.");

	public static SetStorage RegisterStorage<T>(IdDictionary search)
	{
		var storage = new SetStorage(++storageCount);
		GenericData<T>.Storage = storage;

		Array.Resize(ref storages, (int)BitOperations.RoundUpToPowerOf2((uint)(storage.Id /*+ 1*/)));
		GetStorageData(storage) = new StorageData(search);

		return storage;
	}

	public static bool Has(SetStorage setStorage, ContentSet set, int entryId)
	{
		var storage = GetStorageData(setStorage).Sets;
		if (set.Id >= storage.Length)
			return false;

		return storage[set.Id].Values.GetSafe(entryId);
	}

	public static ReadOnlySpan<BitMask64> Values(SetStorage setStorage, ContentSet set)
	{
		var storage = GetStorageData(setStorage).Sets;
		if (set.Id >= storage.Length)
			return [];

		return storage[set.Id].Values.Array.AsSpan();
	}

	public static void IncludeSets(ContentSet set, ReadOnlySpan<ContentSet> sets)
	{
		ref var globalSetEntry = ref globalSetData[set.Id];

		globalSetEntry.IncludedSets ??= new();
		foreach (var includedSet in sets) globalSetEntry.IncludedSets.Add(includedSet);

		for (int i = 0; i < storageCount; i++) {
			storages[i].EnsureCapacity();
			storages[i].DirtySetsMasks.Set(set.Id);
		}
	}
	public static void Include(SetStorage setStorage, ContentSet set, ReadOnlySpan<ValueIndex> indices)
	{
		ref var storage = ref GetStorageData(setStorage);
		storage.EnsureCapacity();
		ref var setData = ref storage.Sets[set.Id];

		setData.ManualInclusions ??= new();
		setData.ManualInclusions.EnsureCapacity(setData.ManualInclusions.Count + indices.Length); // Assumes no duplicates.
		foreach (var index in indices) setData.ManualInclusions.Add(index);

		storage.DirtySetsMasks.Set(set.Id);
	}
	public static void IncludeLegacySets(SetStorage setStorage, ContentSet set, ReadOnlySpan<bool[]> legacySets)
	{
		ref var storage = ref GetStorageData(setStorage);
		storage.EnsureCapacity();
		ref var setData = ref storage.Sets[set.Id];

		setData.IncludedLegacySets ??= new();
		foreach (var includedSet in legacySets) setData.IncludedLegacySets.Add(includedSet);

		storage.DirtySetsMasks.Set(set.Id);
	}

	public static void Recalculate()
	{
		var visitedSets = new HashSet<int>(capacity: 4);

		for (int storageId = 0; storageId < storageCount; storageId++) {
			ref var storage = ref storages[storageId];

			var sets = storage.Sets;
			var dirtySetsMasks = storage.DirtySetsMasks;
			int newLength = storage.Search.Count != int.MaxValue ? storage.Search.Count : ((Dictionary<string, int>.KeyCollection)storage.Search.Names).Count;
			int newBitLength = (newLength / BitMask64.BitSize) + 1;

			// Resize all value arrays.
			for (int i = 0; i < storage.Sets.Length; i++) {
				Array.Resize(ref storage.Sets[i].Values.Array, newBitLength);
			}

			visitedSets.Clear();

			void Recursive(int setId)
			{
				ref var glbSetData = ref globalSetData[setId];
				ref var ctxSetData = ref sets[setId];
				ref var values = ref ctxSetData.Values;

				if (!visitedSets.Add(setId))
					throw new InvalidOperationException($"Infinite recursion detected with set '{glbSetData.Name}'.");

				Array.Clear(values.Array);

				if (ctxSetData.IncludedLegacySets != null) {
					foreach (ReadOnlySpan<bool> booleans in ctxSetData.IncludedLegacySets) {
						for (int maskIndex = 0, start = 0, end = Math.Min(BitMask64.BitSize, booleans.Length);
							maskIndex < values.Array.Length;
							maskIndex++, start = end, end = Math.Min(end + BitMask64.BitSize, booleans.Length)
						) {
							values.Array[maskIndex] |= BitMask64.FromBooleans(booleans[start..end]);
						}
					}
				}

				if (glbSetData.IncludedSets != null) {
					foreach (var innerSet in glbSetData.IncludedSets) {
						var divrem = dirtySetsMasks.DivRem(innerSet.Id);
						if (dirtySetsMasks.Get(divrem)) {
							dirtySetsMasks.Unset(divrem);
							Recursive(innerSet.Id);
						}

						ref readonly var innerSetData = ref sets[innerSet.Id];
						for (int i = 0; i < innerSetData.Values.Array.Length; i++) {
							ctxSetData.Values.Array[i] |= innerSetData.Values.Array[i];
						}
					}
				}

				if (ctxSetData.ManualInclusions != null) {
					foreach (var index in ctxSetData.ManualInclusions) values.Set(index);
				}
			}

			// Recalculate all modified sets.
			for (int maskIndex = 0; maskIndex < dirtySetsMasks.Array.Length; maskIndex++) {
				foreach (int bitIndex in dirtySetsMasks.Array[maskIndex]) {
					// Double check because the iterators use out-of-date masks.
					if (dirtySetsMasks.Get((maskIndex, bitIndex))) {
						dirtySetsMasks.Unset((maskIndex, bitIndex));
						Recursive((maskIndex * BitMask64.BitSize) + bitIndex);
					}
				}
			}
		}
	}
}

public static class ContentSetExtensions
{
	public static bool Has(this ContentSet set, NPC entity) => set.Has<NPCID>(entity.type);
	public static bool Has(this ContentSet set, Item entity) => set.Has<ItemID>(entity.type);
	public static bool Has(this ContentSet set, Projectile entity) => set.Has<ProjectileID>(entity.type);

	public static bool HasTile(this ContentSet set, Tile tile) => set.Has<TileID>(tile.TileType);
	public static bool HasWall(this ContentSet set, Tile tile) => set.Has<WallID>(tile.WallType);
	public static bool HasTile(this ContentSet set, ushort type) => set.Has<TileID>(type);
	public static bool HasWall(this ContentSet set, ushort type) => set.Has<WallID>(type);
}
