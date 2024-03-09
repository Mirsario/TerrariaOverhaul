using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

// Or 'Roll Jumps'. A bunnyhop performed while roll-landing.
public sealed class PlayerBunnyrolls : ModPlayer, IPlayerOnBunnyhopHook
{
	public static readonly SoundStyle BunnyrollSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/Bunnyroll") {
		Volume = 0.9f,
		PitchVariance = 0.2f,
	};
	
	public void OnBunnyhop(Player player, ref float boostAdd, ref float boostMultiplier)
	{
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
			var playerCenter = Player.Center;
			var entitySource = Player.GetSource_FromThis();

			int effectCount = (int)MathF.Ceiling(fallBoost * 5f);

			for (int i = 0; i < effectCount; i++) {
				int ii = i % 3;
				var position = playerCenter + new Vector2(Main.rand.NextFloat(-4f, 4f), 8f);
				var velocity = new Vector2(
					(ii - 1) * fallBoost * Main.rand.NextFloat(0.75f, 1.0f),
					Main.rand.NextFloat(-0.25f, -0.6f)
				);

				velocity = velocity.RotatedByRandom(MathHelper.ToRadians(15f));

				Gore.NewGorePerfect(entitySource, position, velocity, GoreID.Smoke1 + ii);
			}

			SoundEngine.PlaySound(BunnyrollSound.WithVolumeScale(Player.IsLocal() ? 1f : 0.5f), playerCenter);
		}
	}
}
