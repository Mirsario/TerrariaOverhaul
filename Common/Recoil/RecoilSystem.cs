using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Recoil
{
	[Autoload(Side = ModSide.Client)]
	public sealed class RecoilSystem : ModSystem
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

		public static readonly ConfigEntry<bool> EnableAimingRecoil = new(ConfigSide.ClientOnly, "Guns", nameof(EnableAimingRecoil), () => false);

		private static readonly List<CursorOffset> offsets = new();

		public override void Load()
		{
			Main.OnPostDraw += PostDraw;
		}

		public override void Unload()
		{
			Main.OnPostDraw -= PostDraw;
		}

		private void PostDraw(GameTime gameTime)
		{
			if (!EnableAimingRecoil.Value) {
				offsets.Clear();

				return;
			}

			var mouseState = Mouse.GetState();
			var mousePos = new Vector2Int(mouseState.X, mouseState.Y);
			var newMousePos = mousePos;

			for (int i = 0; i < offsets.Count; i++) {
				var info = offsets[i];
				var remaining = info.remainder;
				var newRemainder = remaining * (1f - (float)gameTime.ElapsedGameTime.TotalSeconds * info.speed);

				const float Threshold = 0.01f;

				if (Math.Abs(newRemainder.X) < Threshold) {
					newRemainder.X = 0f;
				}

				if (Math.Abs(newRemainder.Y) < Threshold) {
					newRemainder.Y = 0f;
				}

				info.counter += remaining - newRemainder;
				info.remainder = newRemainder;

				var flooredGain = (Vector2Int)Vector2Utils.Floor(info.counter);

				newMousePos += flooredGain;
				info.counter -= flooredGain;

				if (info.remainder == default) {
					offsets.RemoveAt(i--);
				} else {
					offsets[i] = info;
				}
			}

			if (mousePos != newMousePos) {
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
