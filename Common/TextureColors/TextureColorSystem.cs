using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.TextureColors
{
	/// <summary> This system provides information about average colors of texturse. </summary>
	[Autoload(Side = ModSide.Client)]
	public class TextureColorSystem : ModSystem
	{
		private Dictionary<Asset<Texture2D>, Color> cache;

		public override void Load() => cache = new Dictionary<Asset<Texture2D>, Color>();
		public override void Unload()
		{
			if (cache != null) {
				cache.Clear();

				cache = null;
			}
		}

		/// <summary> Returns an average color from a texture's pixel data. </summary>
		public static Color GetAverageColor(Asset<Texture2D> texture)
		{
			var instance = ModContent.GetInstance<TextureColorSystem>() ?? throw new InvalidOperationException($"{nameof(TextureColorSystem)} has not been loaded.");

			if (!instance.cache.TryGetValue(texture, out var color)) {
				instance.cache[texture] = color = CalculateAverageColor(texture.Value);
			}

			return color;
		}

		/// <summary> Calculates an average color from a texture's pixel data. </summary>
		private static Color CalculateAverageColor(Texture2D tex, byte alphaTest = 64, Rectangle rect = default, HashSet<Color> excludedColors = null)
		{
			bool hasRect = rect != default;
			bool hasExcludedColors = excludedColors != null;
			int texWidth = tex.Width;

			var data = new Color[tex.Width * tex.Height];

			tex.GetData(data);

			long[] values = new long[3];
			long numFittingPixels = 0;

			for (int i = 0; i < data.Length; i++) {
				if (hasRect) {
					int y = i / texWidth;
					int x = i - (y * texWidth);

					if (x < rect.X || y < rect.Y || x >= rect.X + rect.Width || y >= rect.Y + rect.Height) {
						continue;
					}
				}

				var col = data[i];

				if (col.A >= alphaTest) {
					if (hasExcludedColors && excludedColors.Contains(col)) {
						continue;
					}

					values[0] += col.R;
					values[1] += col.G;
					values[2] += col.B;

					numFittingPixels++;
				}
			}

			Color result;

			if (numFittingPixels == 0) {
				result = Color.Transparent;
			} else {
				result = new Color(
					(byte)(values[0] / numFittingPixels),
					(byte)(values[1] / numFittingPixels),
					(byte)(values[2] / numFittingPixels),
					255
				);
			}

			return result;
		}
	}
}
