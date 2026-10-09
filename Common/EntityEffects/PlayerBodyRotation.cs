// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.EntityEffects;

internal sealed class PlayerBodyRotation : ModPlayer
{
	public static readonly ConfigEntry<bool> EnablePlayerTilting = new(ConfigSide.ClientOnly, true, "Visuals");

	public float Rotation;
	private float lastRotation = 0f;
	public float RotationOffsetScale;

	public override void PreUpdate()
	{
		if (Player.dead) {
			return;
		}

		Player.fullRotationOrigin = new Vector2(11, 22);
	}

	public override void PostUpdate()
	{
		if (Player.sleeping.isSleeping) {
			return;
		}

		// Do nothing for minecarts.
		if (Player.mount is { Active: true, Cart: true }) {
			return;
		}

		if(EnablePlayerTilting){

			if (RotationOffsetScale != 0f) {
				float movementRotation = BodyTilting.CalculateRotationOffset(Player.velocity, Player.OnGround(), airMultiplier: 0.8f);

				if (Player.mount.Active) {
					// Reduce intensity on mounts.
					movementRotation *= 0.5f;
				}

				Rotation += movementRotation;

				//TODO: If swimming, multiply by 4.
			}
		// Prevents overriding rotation from other mods if player tilting is disabled and TO isn't providing it's own rotation
		}else if(Rotation == 0f){
			if(lastRotation != 0f) {
				Player.fullRotation = 0f;
			}
			lastRotation = Rotation;
			return;
		}
		Player.fullRotation = Rotation * Player.gravDir;
		
		lastRotation=Rotation;
		Rotation = 0f;
		RotationOffsetScale = 1f;
	}
}
