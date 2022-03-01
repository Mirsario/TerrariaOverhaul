﻿using System;
using Terraria;

namespace TerrariaOverhaul.Utilities
{
	/// <summary> A game tick based timer. Saves a lot of troubles caused by entity component execution orders. </summary>
	public struct Timer
	{
		private uint endTime;

		public bool Active => Main.GameUpdateCount < endTime;
		public int UnclampedValue => (int)((long)endTime - Main.GameUpdateCount);

		public uint Value {
			get => (uint)Math.Max(0, UnclampedValue);
			set => endTime = Main.GameUpdateCount + Math.Max(0, value);
		}

		public void Set(uint minValue) => Value = Math.Max(minValue, Value);

		public static implicit operator Timer(uint value) => new() { Value = value };
		public static implicit operator Timer(int value) => new() { Value = (uint)value };
	}
}
