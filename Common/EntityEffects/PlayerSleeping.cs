// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.ModLoader;
using TerrariaOverhaul.Api.Camera;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.EntityEffects;

internal sealed class PlayerSleeping : ModPlayer
{
	public override void PostUpdate()
	{
		if (!Player.IsLocal() || !Player.sleeping.isSleeping) {
			return;
		}

		CameraCurios.Create(new() {
			Identifier = "Sleeping",
			Weight = 2f,
			LengthInSeconds = 1f,
			Position = Player.Center,
			Zoom = 2f,
		});
	}
}
