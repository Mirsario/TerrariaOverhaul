using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

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

public readonly struct DataEntity
{
	public static readonly DataEntity Invalid = default;

	public readonly uint Id;
	public readonly bool IsValid => Id != 0;

	internal DataEntity(uint id) => Id = id;

	public bool Has<T>() where T : IComponent
		=> DataStorage.HasComponent<T>(this);

	public ref T Get<T>() where T : IComponent
		=> ref DataStorage.GetComponent<T>(this);

	public ref T Add<T>(in T value) where T : IComponent
		=> ref DataStorage.AddComponent(this, in value);

	public void Add(Component component, object value)
		=> DataStorage.AddComponent(this, component, value);
}

internal static class DataStorage
{
	private static class ComponentData<T> where T : IComponent
	{
		public static Component Handle = Component.Invalid;
		public static SparseSet<T> SparseSet = new();

		static ComponentData()
		{
			Handle = new(componentCount++);
			if (Handle.Id >= components.Length)
				Array.Resize(ref components, (int)BitOperations.RoundUpToPowerOf2((uint)(components.Length + 1)));

			components[Handle.Id] = new() {
				Type = typeof(T),
				AddComponentFromObject = Add,
			};
			componentByType[typeof(T)] = Handle;
			componentByName[typeof(T).Name] = Handle;
		}

		public static void Add(DataEntity entity, object value) => SparseSet.Put(entity.Id, (T)value);
	}
	private struct ComponentInfo
	{
		public required Type Type;
		public required Action<DataEntity, object> AddComponentFromObject;
	}

	private static uint entityCount;
	private static uint componentCount;
	private static ComponentInfo[] components = [];
	private static readonly Dictionary<Type, Component> componentByType = [];
	private static readonly Dictionary<string, Component> componentByName = [];

	public static uint EntityCount => entityCount;

	// Entities

	public static DataEntity CreateEntity()
		=> new(entityCount++);

	public static IEnumerable<DataEntity> AllEntities()
	{
		for (uint i = 0; i < entityCount; i++) {
			yield return new DataEntity(i);
		}
	}

	// Components

	public static void RegisterComponent(Type type)
		=> RuntimeHelpers.RunClassConstructor(typeof(ComponentData<>).MakeGenericType(type).TypeHandle);

	public static void RegisterComponent<T>() where T : IComponent
		=> RuntimeHelpers.RunClassConstructor(typeof(ComponentData<T>).TypeHandle);

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
		=> ComponentData<T>.SparseSet.Has(entity.Id);

	public static ref T GetComponent<T>(DataEntity entity) where T : IComponent
		=> ref ComponentData<T>.SparseSet.Get(entity.Id);

	public static ref T AddComponent<T>(DataEntity entity, in T value) where T : IComponent
		=> ref ComponentData<T>.SparseSet.Put(entity.Id, in value);

	public static void AddComponent(DataEntity entity, Component component, object value)
		=> components[component.Id].AddComponentFromObject(entity, value);
}
