using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrariaOverhaul.Utilities;

internal static class Texture2DExtensions
{
	public static void InitializeWithColor(this Texture2D texture, Color color)
	{
		var data = new Color[texture.Width * texture.Height];

		Array.Fill(data, color);

		texture.SetData(data);
	}
}
