using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.Systems.CursorOffsets
{
	[Autoload(Side = ModSide.Client)]
	public sealed class CursorOffsetSystem : ModSystem
	{
		private struct CursorOffset
		{
			public float speed;
			public Vector2 remainder;
			public Vector2 counter;

			public CursorOffset(Vector2 length, float speed)
			{
				this.speed = speed;
				remainder = length;
				counter = default;
			}
		}

		private static List<CursorOffset> offsets;

		public override void Load()
		{
			offsets = new List<CursorOffset>();

			Main.OnPostDraw += PostDraw;
		}
		public override void Unload()
		{
			offsets = null;

			Main.OnPostDraw -= PostDraw;
		}

		private void PostDraw(GameTime gameTime)
		{
			var mouseState = Mouse.GetState();
			var mousePos = new Vector2Int(mouseState.X, mouseState.Y);
			var newMousePos = mousePos;

			for(int i = 0; i < offsets.Count; i++) {
				var info = offsets[i];
				var remaining = info.remainder;
				var newRemainder = remaining * (1f - (float)gameTime.ElapsedGameTime.TotalSeconds * info.speed);

				const float Threshold = 0.01f;

				if(Math.Abs(newRemainder.X) < Threshold) {
					newRemainder.X = 0f;
				}

				if(Math.Abs(newRemainder.Y) < Threshold) {
					newRemainder.Y = 0f;
				}

				info.counter += remaining - newRemainder;
				info.remainder = newRemainder;

				var flooredGain = (Vector2Int)Vector2Utils.Floor(info.counter);

				newMousePos += flooredGain;
				info.counter -= flooredGain;

				if(info.remainder == default) {
					offsets.RemoveAt(i--);
				} else {
					offsets[i] = info;
				}
			}

			if(mousePos != newMousePos) {
				Mouse.SetPosition(newMousePos.X, newMousePos.Y);

				Main.mouseX = newMousePos.X;
				Main.mouseY = newMousePos.Y;
			}
		}

		public static void AddCursorOffset(Vector2 offset, float speed)
		{
			offsets.Add(new CursorOffset(offset, speed));
		}
	}
}
