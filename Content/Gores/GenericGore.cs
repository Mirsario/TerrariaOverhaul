// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;

namespace TerrariaOverhaul.Content.Gores;

internal class GenericGore : ModGore
{
	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		gore.Frame = new SpriteFrame(1, 3, 0, (byte)Main.rand.Next(3));
		gore.light = 0f;
		gore.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
		gore.drawOffset = new Vector2(0f, 4f);
	}

	public override Color? GetAlpha(Gore gore, Color lightColor) => (gore as OverhaulGore)?.BleedColor?.MultiplyRGB(lightColor);
}
