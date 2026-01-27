// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities.Terraria;

/// <summary> A game tick based timer. Saves a lot of troubles caused by entity component execution orders. </summary>
internal struct GameTimer
{
	private static uint RealTime => Main.GameUpdateCount;

	public uint StartTime { get; private set; }
	public uint EndTime { get; private set; }
	public uint? FreezeTime { get; private set; }

	public readonly bool Frozen => FreezeTime.HasValue;
	public readonly bool Active => !FreezeTime.HasValue && CurrentTime < EndTime;
	public readonly uint CurrentTime => FreezeTime ?? RealTime;
	public readonly int UnclampedValue => (int)((long)EndTime - CurrentTime);
	public readonly int UnclampedUnfrozenValue => (int)((long)EndTime - RealTime); // Shenanigans
	public readonly uint Length => Math.Max(0, EndTime - StartTime);

	public uint Value {
		readonly get => (uint)Math.Max(0, UnclampedValue);
		set {
			StartTime = CurrentTime;
			EndTime = StartTime + Math.Max(0, value);
		}
	}

	public readonly float Progress {
		get {
			uint value = Value;
			uint length = Length;

			return length != 0 ? (1f - MathHelper.Clamp(value / (float)length, 0f, 1f)) : 0f;
		}
	}

	public readonly override bool Equals(object? obj) => obj is GameTimer other && this == other;
	public readonly override int GetHashCode() => (int)(StartTime ^ EndTime);

	public void Set(uint minValue)
	{
		Unfreeze();

		StartTime = RealTime;
		Value = Math.Max(minValue, Value);
	}

	public void Freeze()
	{
		Unfreeze();

		FreezeTime = RealTime;
	}

	public void Unfreeze()
	{
		if (!FreezeTime.HasValue) {
			return;
		}

		uint delta = RealTime - FreezeTime.Value;

		StartTime += delta;
		EndTime += delta;
		FreezeTime = null;
	}

	public static implicit operator GameTimer(uint value) => new() { Value = value };
	public static implicit operator GameTimer(int value) => new() { Value = (uint)value };

	public static bool operator ==(GameTimer left, GameTimer right) => left.StartTime == right.StartTime && left.EndTime == right.EndTime;
	public static bool operator !=(GameTimer left, GameTimer right) => left.StartTime != right.StartTime || left.EndTime != right.EndTime;
}
