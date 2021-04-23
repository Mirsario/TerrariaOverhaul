using System;
using Terraria;

namespace TerrariaOverhaul.Utilities.DataStructures
{
	/// <summary> A game tick based timer. Saves a lot of troubles caused by entity component execution orders. </summary>
	public struct Timer
	{
		private uint endTime;

		public bool Active => Main.GameUpdateCount < endTime;
		public uint Value {
			get => (uint)Math.Max(0, (long)endTime - Main.GameUpdateCount);
			set => endTime = Main.GameUpdateCount + Math.Max(0, value);
		}

		public void Set(uint minValue) => Value = Math.Max(minValue, Value);

		public static implicit operator Timer(uint value) => new() { Value = value };
		public static implicit operator Timer(int value) => new() { Value = (uint)value };
	}
}
