using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Components
{
	public sealed class ModComponentContainer<TEntity, TComponent> : IReadOnlyList<TComponent>, IDisposable
		where TEntity : class
		where TComponent : ModComponent<TEntity>
	{
		public TEntity Entity { get; private set; }
		public bool IsDisposed { get; private set; }

		private List<TComponent> Components { get; set; }
		private IReadOnlyList<TComponent> ComponentsReadOnly { get; set; }

		public int Count => Components.Count;

		public TComponent this[int index] => Components[index];

		public ModComponentContainer(TEntity entity, bool autoloadGlobalComponents = true)
		{
			Entity = entity;
			ComponentsReadOnly = (Components = new()).AsReadOnly();

			if (!autoloadGlobalComponents) {
				return;
			}

			foreach (var mod in ModLoader.Mods) {
				foreach (var component in mod.GetContent<TComponent>()) {
					if (component.GetType().GetCustomAttribute<GlobalComponentAttribute>() != null) {
						Add((TComponent)component.Clone());
					}
				}
			}
		}

		public TComponent Add(TComponent component)
		{
			component.OnInit(Entity);

			Components.Add(component);

			return component;
		}

		public bool Has<T>() where T : TComponent
		{
			foreach (var component in Components) {
				if (component is T) {
					return true;
				}
			}

			return false;
		}

		public T Get<T>() where T : TComponent
		{
			foreach (var component in Components) {
				if (component is T result) {
					return result;
				}
			}

			throw new KeyNotFoundException($"Component of type '{typeof(T).Name}' does not exist in the current container.");
		}

		public IEnumerator<TComponent> GetEnumerator() => ComponentsReadOnly.GetEnumerator();

		public void Dispose()
		{
			if (IsDisposed) {
				return;
			}

			foreach (var component in Components) {
				component.OnDispose(Entity);
			}

			Components.Clear();

			Components = null;
			ComponentsReadOnly = null;
			IsDisposed = true;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
