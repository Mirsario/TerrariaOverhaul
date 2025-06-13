using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using BitMask64 = TerrariaOverhaul.Utilities.BitMask<ulong>;

namespace TerrariaOverhaul.Core.Data;

// A basic Entity-Component data storage, meant for entirely immutable use.

public interface IComponent { }

public readonly struct Component
{
	public static readonly Component Invalid = default;

	public readonly uint Id;
	public readonly bool IsValid => Id != 0;

	internal Component(uint id) => Id = id;

	public Type GetComponentType() => DataStorage.GetComponentType(this);
}

public unsafe readonly ref struct ComponentMask
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

public readonly struct DataEntity
{
	public static readonly DataEntity Invalid = default;

	public readonly uint Index;
	public readonly uint Version;
	public readonly bool IsValid => DataStorage.IsEntityValid(this);

	internal DataEntity(uint index, uint version) => (Index, Version) = (index, version);

	public bool Has(ComponentMask mask) => DataStorage.HasComponents(this, mask);
	public bool Has<T>() where T : IComponent => DataStorage.HasComponent<T>(this);
	public ref T Get<T>() where T : IComponent => ref DataStorage.GetComponent<T>(this);
	public ref T Add<T>(in T value) where T : IComponent => ref DataStorage.AddComponent(this, in value);
	public void Add(Component component, object value) => DataStorage.AddComponent(this, component, value);
}

public readonly struct Query
{
	public readonly uint Index;

	internal Query(uint index) => Index = index;

	public Iterator GetEnumerator() => new(this, GetMask());
	public ComponentMask GetMask() => DataStorage.GetQueryComponentMask(this);

	public Query With<T>() where T : IComponent
	{
		GetMask().Set<T>();
		return this;
	}

	public ref struct Iterator
	{
		public readonly Query Query;
		public readonly ComponentMask ComponentMask;
		public BitMask64 PresenceMask;
		public int PresenceMaskIndex = -1;
		public int BaseIndex = -BitMask64.BitSize;

		public DataEntity Current { get; private set; }

		public Iterator(Query query, ComponentMask componentMask) : this()
		{
			Query = query;
			ComponentMask = componentMask;
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

				if (Current.Has(ComponentMask))
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
				AddComponentFromObject = Add,
			};
			componentByType[typeof(T)] = Handle;
			componentByName[typeof(T).Name] = Handle;
		}

		public static void Add(DataEntity entity, object value) => AddComponent(entity, (T)value);
	}
	private struct ComponentInfo
	{
		public required Type Type;
		public required Action<DataEntity, object> AddComponentFromObject;
	}

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
	private static BitMask64[] queryComponentMasks = [];

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
			GrowComponentMasks(ref queryComponentMasks, queryCount);
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
		foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IComponent).IsAssignableFrom(t))) {
			RegisterComponent(type);
		}
	}

	// Entity component data

	public static bool HasComponent<T>(DataEntity entity) where T : IComponent
		=> ComponentData<T>.SparseSet.Has(entity.Index);

	public static bool HasComponents(DataEntity entity, ComponentMask mask)
	{
		if (componentMasksPerEntity == 1)
			return (mask.Span[0] & entityComponentMasks[entity.Index]) == mask.Span[0];

		uint baseIndex = entity.Index * componentMasksPerEntity;
		for (int i = 0; i < componentMasksPerEntity; i++) {
			if ((mask.Span[i] & entityComponentMasks[baseIndex + i]) != mask.Span[i]) {
				return false;
			}
		}
		return true;
	}

	public static ref T GetComponent<T>(DataEntity entity) where T : IComponent
		=> ref ComponentData<T>.SparseSet.Get(entity.Index);

	public static void AddComponent(DataEntity entity, Component component, object value)
		=> components[component.Id].AddComponentFromObject(entity, value);

	public static ref T AddComponent<T>(DataEntity entity, in T value) where T : IComponent
	{
		ComponentMask.InDynamicArray(entityComponentMasks, entity.Index).Set<T>();
		return ref ComponentData<T>.SparseSet.Put(entity.Index, in value);
	}
	public static void RemoveComponent<T>(DataEntity entity) where T : IComponent
	{
		ComponentMask.InDynamicArray(entityComponentMasks, entity.Index).Unset<T>();
		ComponentData<T>.SparseSet.Remove(entity.Index);
	}

	// Entities

	public static DataEntity CreateEntity()
	{
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
		uint index = queryCount++;
		Array.Resize(ref queryComponentMasks, (int)(BitOperations.RoundUpToPowerOf2(index + 1) * componentMasksPerEntity));
		return new(index);
	}

	internal static ComponentMask GetQueryComponentMask(Query query)
		=> ComponentMask.InDynamicArray(queryComponentMasks, query.Index);
}
