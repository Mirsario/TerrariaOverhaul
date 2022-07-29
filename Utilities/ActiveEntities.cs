using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;

namespace TerrariaOverhaul.Utilities;

// Tons of shenanigans that intend to optimize enumerations of active entities.
// ...inb4 it's worse.
public static class ActiveEntities
{
	public struct EntityEnumerator<T> : IEnumerable<T>, IEnumerator<T>
	{
		private readonly T[] array;
		private readonly int arrayLength;
		private int index;

		public T Current { get; private set; }

		object IEnumerator.Current => Current!;

		[MethodImpl(OptimizationFlags)]
		public EntityEnumerator(T[] entities, int length)
		{
			array = entities;
			arrayLength = length;

			index = -1;
			Current = default!;
		}

		[MethodImpl(OptimizationFlags)]
		public bool MoveNext()
		{
			while (true) {
				index++;

				if (index >= arrayLength) {
					Current = default!;
					return false;
				}

				var entity = array[index];

				if (IsActive(entity)) {
					Current = entity;
					return true;
				}
			}
		}

		public void Reset()
		{
			index = -1;
			Current = default!;
		}

		[MethodImpl(OptimizationFlags)]
		public EntityEnumerator<T> GetEnumerator() => this;

		IEnumerator IEnumerable.GetEnumerator() => this;
		
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
		
		void IDisposable.Dispose() { }
	}

	private struct EntityArrayWrapper<T>
	{
		private readonly T[] array;
		private readonly T[] sourceArray;

		private int length;
		private int maxLength;
		private uint lastArrayUpdateTick;
		private EntityEnumerator<T> enumerator;

		public EntityArrayWrapper(T[] sourceArray, int maxLength)
		{
			length = 0;
			this.maxLength = maxLength;
			
			array = new T[maxLength];
			this.sourceArray = sourceArray;

			enumerator = default;
			lastArrayUpdateTick = uint.MaxValue;
		}

		public EntityEnumerator<T> GetEnumerator()
		{
			if (Main.GameUpdateCount != lastArrayUpdateTick) {
				length = 0;
				lastArrayUpdateTick = Main.GameUpdateCount;

				for (int i = 0; i < maxLength; i++) {
					var entry = sourceArray[i];

					if (entry != null && IsActive(entry)) {
						array[length++] = entry;
					}
				}

				enumerator = new EntityEnumerator<T>(array, length);
			}

			return enumerator;
		}
	}
	
	private const MethodImplOptions OptimizationFlags = MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization;

	private static EntityArrayWrapper<NPC> npcs = new(Main.npc, Main.maxNPCs);
	private static EntityArrayWrapper<Gore> gores = new(Main.gore, Main.maxGore);
	private static EntityArrayWrapper<Dust> dusts = new(Main.dust, Main.maxDust);
	private static EntityArrayWrapper<Item> items = new(Main.item, Main.maxItems);
	private static EntityArrayWrapper<Player> players = new(Main.player, Main.maxPlayers);

	public static EntityEnumerator<NPC> NPCs => npcs.GetEnumerator();
	public static EntityEnumerator<Gore> Gores => gores.GetEnumerator();
	public static EntityEnumerator<Dust> Dusts => dusts.GetEnumerator();
	public static EntityEnumerator<Item> Items => items.GetEnumerator();
	public static EntityEnumerator<Player> Players => players.GetEnumerator();

	// This should be inlined as just the return value.
	[MethodImpl(OptimizationFlags)]
	private static bool IsActive<T>(T t) => t switch {
		Entity entity => entity.active,
		Gore gore => gore.active,
		Dust dust => dust.active,
		_ => throw new NotImplementedException()
	};
}
