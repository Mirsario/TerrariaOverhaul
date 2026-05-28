using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using BitMask64 = TerrariaOverhaul.Utilities.BitMask<ulong>;

namespace TerrariaOverhaul.Core.Data;

internal interface IComponent { }

internal readonly struct Component
{
	public static readonly Component Invalid = default;

	public readonly uint Id;
	public readonly bool IsValid => Id != 0;

	internal Component(uint id) => Id = id;

	public Type GetComponentType() => DataStorage.GetComponentType(this);
}

internal unsafe readonly ref struct ComponentMask
{
	public readonly Span<BitMask64> Span;

	public static int ExpectedLength => (int)DataStorage.ComponentMasksPerEntity;

	private ComponentMask(Span<BitMask64> span) => Span = span;

	public void Set<T>() where T : IComponent
	{
		var (div, rem) = Math.DivRem(DataStorage.GetComponentIndex<T>(), BitMask64.BitSize);
		Span[(int)div].Set((int)rem);
	}
	public void Unset<T>() where T : IComponent
	{
		var (div, rem) = Math.DivRem(DataStorage.GetComponentIndex<T>(), BitMask64.BitSize);
		Span[(int)div].Unset((int)rem);
	}

	public static ComponentMask InSpan(Span<BitMask64> span) => new(span);
	public static ComponentMask InDynamicArray(BitMask64[] arr, uint entryIndex)
		=> new(new Span<BitMask64>(arr, (int)(entryIndex * ExpectedLength), (int)ExpectedLength));
}

internal readonly struct DataEntity
{
	public static readonly DataEntity Invalid = default;

	public readonly uint Index;
	public readonly uint Version;
	public readonly bool IsValid => DataStorage.IsEntityValid(this);

	internal DataEntity(uint index, uint version) => (Index, Version) = (index, version);

	public bool Has<T>() where T : IComponent => DataStorage.HasComponent<T>(this);
	public ref T Get<T>() where T : IComponent => ref DataStorage.GetComponent<T>(this);
	public ref T Add<T>(in T value) where T : IComponent => ref DataStorage.AddComponent(this, in value);
	public void Remove<T>() where T : IComponent => DataStorage.RemoveComponent<T>(this);
	public bool HasAll(ComponentMask mask) => DataStorage.HasAllComponents(this, mask);
	public bool HasAny(ComponentMask mask) => DataStorage.HasAnyComponents(this, mask);
	public void AddByHandle(Component component, object value) => DataStorage.AddComponent(this, component, value);
	public void Clear() => DataStorage.ClearEntity(this);
	public void Destroy() => DataStorage.DestroyEntity(this);
}

internal readonly struct Query
{
	public readonly uint Index;

	internal Query(uint index) => Index = index;

	public Iterator GetEnumerator() => new(GetIncludedMask(), GetExcludedMask());
	public ComponentMask GetIncludedMask() => DataStorage.GetQueryIncludedComponentMask(this);
	public ComponentMask GetExcludedMask() => DataStorage.GetQueryExcludedComponentMask(this);

	public Query With<T>() where T : IComponent
	{
		GetIncludedMask().Set<T>();
		return this;
	}
	public Query Without<T>() where T : IComponent
	{
		GetExcludedMask().Set<T>();
		return this;
	}

	public ref struct Iterator
	{
		public readonly ComponentMask IncludedComponents;
		public readonly ComponentMask ExcludedComponents;
		public BitMask64 PresenceMask;
		public int PresenceMaskIndex = -1;
		public int BaseIndex = -BitMask64.BitSize;

		public DataEntity Current { get; private set; }

		public Iterator(ComponentMask includedComponents, ComponentMask excludedComponents) : this()
		{
			IncludedComponents = includedComponents;
			ExcludedComponents = excludedComponents;
		}

		public bool MoveNext()
		{
			while (true) {
				if (PresenceMask.IsZero) {
					if (++PresenceMaskIndex >= DataStorage.EntityPresenceMasks.Length) {
						Current = default;
						return false;
					}
					BaseIndex += BitMask64.BitSize;
					PresenceMask = DataStorage.EntityPresenceMasks[PresenceMaskIndex];
					continue;
				}

				int bitIndex = PresenceMask.TrailingZeroCount();
				PresenceMask.Unset(bitIndex);

				uint entityIndex = (uint)(BaseIndex + bitIndex);
				Current = new DataEntity(entityIndex, DataStorage.EntityVersions[entityIndex]);

				if (Current.HasAll(IncludedComponents) && !Current.HasAny(ExcludedComponents))
					return true;
			}
		}
	}
}

internal static class DataStorage
{
	private static class ComponentData<T> where T : IComponent
	{
		public static Component Handle = Component.Invalid;
		public static SparseSet<T> SparseSet = new();

		static ComponentData()
		{
			Handle = CreateComponent();

			components[Handle.Id] = new() {
				Type = typeof(T),
				AddFromObject = Add,
				Remove = Remove,
			};
			componentByType[typeof(T)] = Handle;
			componentByName[typeof(T).Name] = Handle;
		}

		public static void Add(DataEntity entity, object value) => AddComponent(entity, (T)value);
		public static void Remove(DataEntity entity) => RemoveComponent<T>(entity);
	}
	private struct ComponentInfo
	{
		public required Type Type;
		public required Action<DataEntity, object> AddFromObject;
		public required Action<DataEntity> Remove;
	}

	// These should have almost no cost unless a second thread kicks in.
	private static readonly object operationLock = new();
	// Components
	private static uint componentCount;
	private static uint componentMasksPerEntity = 1;
	private static ComponentInfo[] components = [];
	private static readonly Dictionary<Type, Component> componentByType = [];
	private static readonly Dictionary<string, Component> componentByName = [];
	// Entities
	private static uint entityCount;
	private static uint[] entityVersions = [];
	private static BitMask64[] entityPresenceMasks = [];
	private static BitMask64[] entityComponentMasks = [];
	// Queries
	private static uint queryCount;
	private static BitMask64[] queryExcludedComponentMasks = [];
	private static BitMask64[] queryIncludedComponentMasks = [];

	public static uint EntityCount => entityCount;
	public static uint ComponentMasksPerEntity => componentMasksPerEntity;
	internal static uint[] EntityVersions => entityVersions;
	internal static BitMask64[] EntityPresenceMasks => entityPresenceMasks;

	// Components

	public static Component CreateComponent()
	{
		var handle = new Component(componentCount++);
		if (handle.Id >= components.Length)
			Array.Resize(ref components, (int)BitOperations.RoundUpToPowerOf2((uint)(components.Length + 1)));

		uint newMasksPerEntity = (handle.Id / BitMask64.BitSize) + 1;
		if (newMasksPerEntity != componentMasksPerEntity && entityComponentMasks.Length > 0) {
			void GrowComponentMasks(ref BitMask64[] masks, uint entryCount)
			{
				var oldMasks = masks;
				var newMasks = masks = new BitMask64[BitOperations.RoundUpToPowerOf2(entryCount + 1) * newMasksPerEntity];
				for (int i = 0, j = 0; i < oldMasks.Length; i++, j += (i % componentMasksPerEntity == 0) ? 2 : 1) {
					newMasks[j] = oldMasks[i];
				}
			}

			GrowComponentMasks(ref entityComponentMasks, entityCount);
			GrowComponentMasks(ref queryIncludedComponentMasks, queryCount);
			GrowComponentMasks(ref queryExcludedComponentMasks, queryCount);
			componentMasksPerEntity = newMasksPerEntity;
		}

		return handle;
	}

	public static void RegisterComponent(Type type)
		=> RuntimeHelpers.RunClassConstructor(typeof(ComponentData<>).MakeGenericType(type).TypeHandle);

	public static void RegisterComponent<T>() where T : IComponent
		=> RuntimeHelpers.RunClassConstructor(typeof(ComponentData<T>).TypeHandle);

	public static Component GetComponentHandle<T>() where T : IComponent
		=> ComponentData<T>.Handle;

	public static uint GetComponentIndex<T>() where T : IComponent
		=> ComponentData<T>.Handle.Id;

	public static Type GetComponentType(Component component)
		=> components[component.Id].Type;

	public static bool TryGetComponentFromName(string name, [NotNullWhen(true)] out Component result)
		=> componentByName.TryGetValue(name, out result);

	public static void RegisterTypesFromAssembly(Assembly assembly)
	{
		foreach (var type in AssemblyManager.GetLoadableTypes(assembly).Where(t => !t.IsAbstract && typeof(IComponent).IsAssignableFrom(t))) {
			RegisterComponent(type);
		}
	}

	// Entity component data

	public static bool HasComponent<T>(DataEntity entity) where T : IComponent
		=> ComponentData<T>.SparseSet.Has(entity.Index);

	public static bool HasAllComponents(DataEntity entity, ComponentMask mask)
	{
		//	if (componentMasksPerEntity == 1)
		//		return (mask.Span[0] & entityComponentMasks[entity.Index]) == mask.Span[0];

		uint baseIndex = entity.Index * componentMasksPerEntity;
		for (int i = 0; i < componentMasksPerEntity; i++) {
			// Halt with a negative result if AND'ing the masks loses any expected bits.
			if ((mask.Span[i] & entityComponentMasks[baseIndex + i]) != mask.Span[i]) {
				return false;
			}
		}
		return true;
	}
	public static bool HasAnyComponents(DataEntity entity, ComponentMask mask)
	{
		//	if (componentMasksPerEntity == 1)
		//		return !(mask.Span[0] & entityComponentMasks[entity.Index]).IsZero;

		uint baseIndex = entity.Index * componentMasksPerEntity;
		for (int i = 0; i < componentMasksPerEntity; i++) {
			// Halt with a positive result if AND'ing the masks results in anything but a zero.
			if (!(mask.Span[i] & entityComponentMasks[baseIndex + i]).IsZero) {
				return true;
			}
		}
		return false;
	}

	public static ref T GetComponent<T>(DataEntity entity) where T : IComponent
		=> ref ComponentData<T>.SparseSet.Get(entity.Index);

	public static void AddComponent(DataEntity entity, Component component, object value)
	{
		lock (operationLock) {
			components[component.Id].AddFromObject(entity, value);
		}
	}
	public static ref T AddComponent<T>(DataEntity entity, in T value) where T : IComponent
	{
		lock (operationLock) {
			ComponentMask.InDynamicArray(entityComponentMasks, entity.Index).Set<T>();
			return ref ComponentData<T>.SparseSet.Put(entity.Index, in value);
		}
	}
	public static void RemoveComponent<T>(DataEntity entity) where T : IComponent
	{
		Debug.Assert(Program.IsMainThread);
		ComponentMask.InDynamicArray(entityComponentMasks, entity.Index).Unset<T>();
		ComponentData<T>.SparseSet.Remove(entity.Index);
	}
	public static void ClearEntity(DataEntity entity)
	{
		uint baseMask = entity.Index * componentMasksPerEntity;
		for (int maskIndex = 0, baseBit = 0; maskIndex < componentMasksPerEntity; maskIndex++, baseBit += BitMask64.BitSize) {
			foreach (int bitIndex in entityComponentMasks[baseMask + maskIndex]) {
				components[baseBit + bitIndex].Remove(entity);
			}
		}
	}

	// Entities

	public static DataEntity CreateEntity()
	{
		Debug.Assert(Program.IsMainThread);
		
		uint index = entityCount++;
		uint oldLength = BitOperations.RoundUpToPowerOf2(index + 0);
		uint newLength = BitOperations.RoundUpToPowerOf2(index + 1);
		if (newLength != oldLength) {
			Array.Resize(ref entityVersions, (int)newLength);
			Array.Resize(ref entityPresenceMasks, (int)(newLength / BitMask64.BitSize) + 1);
			Array.Resize(ref entityComponentMasks, (int)(newLength * componentMasksPerEntity));
		}

		var (div, rem) = Math.DivRem(index, BitMask64.BitSize);
		entityPresenceMasks[div].Set((int)rem);

		uint version = entityVersions[index];
		if (version == 0) version = ++entityVersions[index];
		
		return new(index, version);
	}
	public static void DestroyEntity(DataEntity entity)
	{
		Debug.Assert(Program.IsMainThread);
		Debug.Assert(entity.IsValid);
		entityVersions[entity.Index]++;
		var (div, rem) = Math.DivRem(entity.Index, BitMask64.BitSize);
		entityPresenceMasks[div].Unset((int)rem);
	}

	public static bool IsEntityValid(DataEntity entity)
		=> entity.Version != 0 && entity.Version == entityVersions[entity.Index];

	// Queries

	public static Query CreateQuery()
	{
		lock (operationLock) {
			uint index = queryCount++;
			int masksArrayLength = (int)(BitOperations.RoundUpToPowerOf2(index + 1) * componentMasksPerEntity);
			Array.Resize(ref queryIncludedComponentMasks, masksArrayLength);
			Array.Resize(ref queryExcludedComponentMasks, masksArrayLength);
			return new(index);
		}
	}

	internal static ComponentMask GetQueryIncludedComponentMask(Query query) => ComponentMask.InDynamicArray(queryIncludedComponentMasks, query.Index);
	internal static ComponentMask GetQueryExcludedComponentMask(Query query) => ComponentMask.InDynamicArray(queryExcludedComponentMasks, query.Index);
}

#if DEBUG
internal sealed class DataTests : ModSystem
{
	public struct Cmp() : IComponent
	{
		public int Int = 123;
	}
	
	public override void OnModLoad()
	{
		Main.QueueMainThreadAction(Test);
	}

	public static void Test()
	{
		var entA = DataStorage.CreateEntity();
		var entB = DataStorage.CreateEntity();
		var entX = new DataEntity();
		Debug.Assert(entA.IsValid);
		Debug.Assert(entB.IsValid);
		Debug.Assert(!entX.IsValid);
		entX = DataStorage.CreateEntity();
		Debug.Assert(entX.IsValid);

		var cmpA = new Component();
		Debug.Assert(!cmpA.IsValid);
		DataStorage.RegisterComponent<Cmp>();
		cmpA = DataStorage.GetComponentHandle<Cmp>();
		Debug.Assert(cmpA.IsValid);

		Debug.Assert(!entX.Has<Cmp>());
		entX.Add(new Cmp { Int = 123 });
		Debug.Assert(entX.Has<Cmp>());
		Debug.Assert(!entA.Has<Cmp>());
		Debug.Assert(!entB.Has<Cmp>());
		entB.Add(new Cmp { Int = 321 });
		Debug.Assert(entX.Has<Cmp>());
		Debug.Assert(!entA.Has<Cmp>());
		Debug.Assert(entB.Has<Cmp>());
		Debug.Assert(entX.Get<Cmp>().Int == 123);
		Debug.Assert(entB.Get<Cmp>().Int == 321);
		entX.Remove<Cmp>();
		Debug.Assert(!entX.Has<Cmp>());
	}
}
#endif
