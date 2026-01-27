// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Utilities.Terraria;

internal static class SpriteBatchUtils
{
	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "samplerState")]
	public extern static ref SamplerState GetSamplerState(this SpriteBatch sb);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static void DrawLine(this SpriteBatch sb, Vector2 start, Vector2 end, Color? color = null, int width = 1, Texture2D? texture = null)
	{
		var offset = end - start;
		float angle = (float)Math.Atan2(offset.Y, offset.X);
		var rect = new Rectangle(
			(int)Math.Round(start.X), (int)Math.Round(start.Y),
			(int)offset.Length(), width
		);

		sb.Draw(texture ?? TextureAssets.BlackTile.Value, rect, null, color ?? Color.White, angle, Vector2.Zero, SpriteEffects.None, 0f);
	}

	// Text
	public static void DrawStringOutlined(this SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 position, Color color, Vector2 origin = default, Vector2? scale = null, Color? outlineColor = null)
	{
		Color newColor = outlineColor ?? Color.Black.WithAlpha(0.5f);

		scale ??= Vector2.One;

		for (int i = 0; i < 5; i++) {
			if (i == 4)
				newColor = color;

			var offset = i switch {
				0 => new Vector2(-2f, +0f),
				1 => new Vector2(+2f, +0f),
				2 => new Vector2(+0f, -2f),
				3 => new Vector2(+0f, +2f),
				_ => default,
			};

			sb.DrawString(font, text, position + offset, newColor, 0f, origin, scale.Value, SpriteEffects.None, 0f);
		}
	}
}
