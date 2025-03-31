// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Content.Dusts;

public class ManaDust : ModDust
{
	public override void OnSpawn(Dust dust)
	{
		dust.noGravity = true;
		dust.frame = new Rectangle(0, 0, 22, 22);
		dust.scale = 0.5f;
	}

	public override bool Update(Dust dust)
	{
		dust.velocity.Y -= 1f / 60f;
		dust.fadeIn += 1f / 60f;

		return true;
	}

	public override Color? GetAlpha(Dust dust, Color lightColor)
	{
		return Color.White.WithAlpha(0.5f);
	}
}
