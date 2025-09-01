// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.EntityEffects;

internal sealed class PlayerItemRotation : ModPlayer
{
	public float? ForcedItemRotation;

	public override void PostUpdate()
	{
		if (ForcedItemRotation.HasValue) {
			Player.itemRotation = ForcedItemRotation.Value;

			ForcedItemRotation = null;
		}
	}
}
