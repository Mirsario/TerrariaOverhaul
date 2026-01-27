// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.GrapplingHooks;

[Autoload(Side = ModSide.Client)]
internal sealed class ItemGrapplingHookTooltips : GlobalItem
{
	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		if (item.shoot <= ProjectileID.None|| !ContentSampleUtils.TryGetProjectile(item.shoot, out var projectile) || projectile.aiStyle != ProjAIStyleID.Hook) {
			return;
		}

		int preferredIndex = tooltips.FindIndex(t => t.Name == "Equipable");
		int usedIndex = preferredIndex >= 0 ? (preferredIndex + 1) : tooltips.Count;

		tooltips.Insert(
			usedIndex,
			new TooltipLine(Mod, $"{nameof(TerrariaOverhaul)}/ClassicGrapplingHookPullTip", Mod.GetTextValue("CommonTooltips.GrapplingHooks.ClassicPullTip")) {
				OverrideColor = Color.LimeGreen
			}
		);
	}
}
