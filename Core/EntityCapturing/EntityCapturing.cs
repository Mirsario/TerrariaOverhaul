// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Core.EntityCapturing;

[Flags]
internal enum SpawnResolution
{
	/// <summary> Allows the entity to spawn, without recording it. </summary>
	Passthrough = 0,
	/// <summary> Prevents the entity from spawning, without recording it. </summary>
	Discard = 1 << 0,
	/// <summary> Allows the entity to spawn, and also records it. </summary>
	Record = 1 << 1,
	/// <summary> Prevents the entity from spawning, recording the parameters. </summary>
	Capture = 1 << 2,
}

internal static class SpawnResolutionExtensions
{
	public static bool Captures(this SpawnResolution res)
		=> (res & (SpawnResolution.Record | SpawnResolution.Capture)) != 0;

	public static bool Discards(this SpawnResolution res)
		=> (res & (SpawnResolution.Discard | SpawnResolution.Capture)) != 0;
}

/// <summary>
/// Records which entities have been created or attempted to be created in a defined scope. 
/// <code> using var _ = EntityCapturing&lt;ItemCapture&gt;.Push(FilterResult.Capture, out var list) </code>
/// </summary>
internal abstract class EntityCapture<TCapture> where TCapture : struct
{
	public delegate SpawnResolution SpawnFilter(in TCapture args);

	public record struct Handle(List<TCapture> Records, SpawnFilter Callback) : IDisposable
	{
		public void Dispose()
		{
			if (this != default) {
				Pop();
				this = default;
			}
		}
	}

	private static readonly Stack<Handle> stack = new();

	public static Handle Push(out List<TCapture> captures, SpawnFilter callback)
	{
		captures = new();
		var handle = new Handle(captures, callback);
		stack.Push(handle);
		return handle;
	}

	public static Handle Push(List<TCapture> captures, SpawnFilter callback)
	{
		captures?.Clear();
		var handle = new Handle(captures, callback);
		stack.Push(handle);
		return handle;
	}

	/// <summary> Captures all spawns until disposed. </summary>
	public static Handle Capture(List<TCapture> captures)
		=> Push(captures, static (in TCapture args) => SpawnResolution.Capture);

	/// <inheritdoc cref="Capture"/>
	public static Handle Capture(out List<TCapture> captures)
		=> Push(out captures, static (in TCapture args) => SpawnResolution.Capture);

	/// <summary> Records all spawns until disposed. </summary>
	public static Handle Record(List<TCapture> captures)
		=> Push(captures, static (in TCapture args) => SpawnResolution.Record);

	/// <inheritdoc cref="Record"/>
	public static Handle Record(out List<TCapture> captures)
		=> Push(out captures, static (in TCapture args) => SpawnResolution.Record);

	/// <summary> Pushes a filter that allows all spawns, preventing captures from other active filters. </summary>
	public static Handle SuspendCaptures()
		=> Push(null!, static (in TCapture args) => SpawnResolution.Passthrough);

	/// <summary> Pushes a filter that discards all spawns, also preventing captures from other active filters. </summary>
	public static Handle SuspendSpawning()
		=> Push(null!, static (in TCapture args) => SpawnResolution.Discard);

	internal static Handle? Peek()
		=> stack.TryPeek(out var result) ? result : null;

	internal static void Pop()
		=> stack.Pop();
}

internal record struct ItemCapture(int Index, IEntitySource Source, RectFloat Area, int Type, int Stack, int Prefix);

internal sealed class ItemCapturing : EntityCapture<ItemCapture>
{
	internal sealed class ItemCapturingImpl : ModSystem
	{
		private static readonly int Dummy = Main.maxItems;

		public override void Load()
		{
			On_Item.NewItem_IEntitySource_int_int_int_int_int_int_bool_int_bool_bool += (
				static (orig, source, x, y, width, height, type, stack, noBroadcast, prefix, noGrabDelay, reverseLookup) => {
					if (Peek() is { } capture) {
						var args = new ItemCapture(Dummy, source, new RectFloat(x, y, width, height), type, stack, prefix);
						switch (capture.Callback(in args)) {
							case SpawnResolution.Discard: return Dummy;
							case SpawnResolution.Capture: capture.Records.Add(args); return Dummy;
							case SpawnResolution.Record:
								args.Index = orig(source, x, y, width, height, type, stack, noBroadcast, prefix, noGrabDelay, reverseLookup);
								capture.Records.Add(args);
								return args.Index;
						}
					}

					return orig(source, x, y, width, height, type, stack, noBroadcast, prefix, noGrabDelay, reverseLookup);
				}
			);
		}
	}
}

internal record struct NpcCapture(int Index, IEntitySource Source, Vector2 Position, int Type, int Start, float AI0, float AI1, float AI2, float AI3, int Target);

internal sealed class NpcCapturing : EntityCapture<NpcCapture>
{
	internal sealed class NpcCapturingImpl : ModSystem
	{
		private static readonly int Dummy = Main.maxNPCs;

		public override void Load()
		{
			On_NPC.NewNPC += (
				static (orig, source, x, y, type, start, ai0, ai1, ai2, ai3, target) => {
					if (Peek() is { } capture) {
						var args = new NpcCapture(Dummy, source, new(x, y), type, start, ai0, ai1, ai2, ai3, target);
						switch (capture.Callback(in args)) {
							case SpawnResolution.Discard: return Dummy;
							case SpawnResolution.Capture: capture.Records.Add(args); return Dummy;
							case SpawnResolution.Record:
								args.Index = orig(source, x, y, type, start, ai0, ai1, ai2, ai3, target);
								capture.Records.Add(args);
								return args.Index;
						}
					}

					return orig(source, x, y, type, start, ai0, ai1, ai2, ai3, target);
				}
			);
		}
	}
}

internal record struct ProjectileCapture(int Index, IEntitySource Source, Vector2 Position, Vector2 Velocity, int Type, int Damage, float Knockback, int Owner, float AI0, float AI1, float AI2);

internal sealed class ProjectileCapturing : EntityCapture<ProjectileCapture>
{
	internal sealed class ProjectileCapturingImpl : ModSystem
	{
		private static readonly int Dummy = Main.maxProjectiles;

		public override void Load()
		{
			On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += (
				static (orig, source, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1, ai2) => {
					if (Peek() is { } capture) {
						var args = new ProjectileCapture(Dummy, source, new(x, y), new(speedX, speedY), type, damage, knockback, owner, ai0, ai1, ai2);
						switch (capture.Callback(in args)) {
							case SpawnResolution.Discard: return Dummy;
							case SpawnResolution.Capture: capture.Records.Add(args); return Dummy;
							case SpawnResolution.Record:
								args.Index = orig(source, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1, ai2);
								capture.Records.Add(args);
								return args.Index;
						}
					}

					return orig(source, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1, ai2);
				}
			);
		}
	}
}

internal record struct GoreCapture(int Index, IEntitySource Source, Vector2 Position, Vector2 Velocity, int Type, float Scale);

internal sealed class GoreCapturing : EntityCapture<GoreCapture>
{
	internal sealed class GoreCapturingImpl : ModSystem
	{
		private static readonly int Dummy = Main.maxGore;

		public override void Load()
		{
			On_Gore.NewGore_IEntitySource_Vector2_Vector2_int_float += (
				static (orig, src, position, velocity, type, scale) => {
					if (Peek() is { } capture) {
						var args = new GoreCapture(Dummy, src, position, velocity, type, scale);
						switch (capture.Callback(in args)) {
							case SpawnResolution.Discard: return Dummy;
							case SpawnResolution.Capture: capture.Records.Add(args); return Dummy;
							case SpawnResolution.Record:
								args.Index = orig(src, position, velocity, type, scale);
								capture.Records.Add(args);
								return args.Index;
						}
					}

					return orig(src, position, velocity, type, scale);
				}
			);
		}
	}
}

internal record struct DustCapture(int Index, RectFloat Area, int Type, Vector2 Velocity, int Alpha, Color Color, float Scale);

internal sealed class DustCapturing : EntityCapture<DustCapture>
{
	internal sealed class DustCapturingImpl : ModSystem
	{
		private static readonly int Dummy = Main.maxDust;

		public override void Load()
		{
			On_Dust.NewDust += (
				static (orig, pos, width, height, type, speedX, speedY, alpha, newColor, scale) => {
					if (Peek() is { } capture) {
						var args = new DustCapture(Dummy, new(pos.X, pos.Y, width, height), type, new(speedX, speedY), alpha, newColor, scale);
						switch (capture.Callback(in args)) {
							case SpawnResolution.Discard: return Dummy;
							case SpawnResolution.Capture: capture.Records.Add(args); return Dummy;
							case SpawnResolution.Record:
								args.Index = orig(pos, width, height, type, speedX, speedY, alpha, newColor, scale);
								capture.Records.Add(args);
								return args.Index;
						}
					}

					return orig(pos, width, height, type, speedX, speedY, alpha, newColor, scale);
				}
			);
		}
	}
}
