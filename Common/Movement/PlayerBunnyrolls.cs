// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Common.Footsteps;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Movement;

// Or 'Roll Jumps'. A bunnyhop performed while roll-landing.
internal sealed class PlayerBunnyrolls : ModPlayer, IPlayerOnBunnyhopHook
{
	public static readonly ConfigEntry<bool> EnableRollJumps = new(ConfigSide.Both, true, "Movement");

	public static readonly SoundStyle BunnyrollSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/Bunnyroll") {
		Volume = 0.9f,
		PitchVariance = 0.2f,
	};
	
	public void OnBunnyhop(Player player, ref float boostAdd, ref float boostMultiplier)
	{
		if (!EnableRollJumps) {
			return;
		}

		if (!Player.TryGetModPlayer(out PlayerDodgerolls dodgerolls) || !dodgerolls.IsDodging) {
			return;
		}

		Player.TryGetModPlayer(out PlayerMovement movement);

		const float MinVerticalSpeed = 6.0f;
		const float SpeedConversion = 0.15f;
		const float MaxBoost = 2.0f;
		const float MinBoost = 0.0f; // 0.190f;

		float verticalSpeed = movement.VelocityRecord.Max(v => v.Y);
		float subtractedSpeed = MathF.Max(0f, verticalSpeed - MinVerticalSpeed);
		float fallBoost = MathHelper.Clamp(subtractedSpeed * SpeedConversion, MinBoost, MaxBoost);

		boostAdd += fallBoost;
		//boostMultiplier += 0.25f;

		if (!Main.dedServ) {
			var playerBottom = Player.Bottom;
			var entitySource = Player.GetSource_FromThis();

			// Produce a particle only if footstep particles are disabled.
			if (!FootstepSystem.EnableMovementDust) {
				var position = playerBottom + new Vector2(Main.rand.NextFloat(-4, 4), 8);
				var velocity = new Vector2(fallBoost * Player.direction * Main.rand.NextFloat(0.25f, 0.50f), 0);
				var goreType = ModContent.GoreType<DustCloudMedium>();

				if (Gore.NewGorePerfect(entitySource, position, velocity, goreType) is { active: true } gore) {
					Main.instance.LoadGore(goreType);
					gore.position -= gore.AABBRectangle.Size() * new Vector2(0.5f, 1.0f);
				}
			}

			SoundEngine.PlaySound(BunnyrollSound.WithVolumeScale(Player.IsLocal() ? 1f : 0.5f), playerBottom);
		}
	}
}
