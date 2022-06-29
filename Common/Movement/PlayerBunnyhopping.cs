using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement
{
	public sealed class PlayerBunnyhopping : ModPlayer
	{
		public static readonly ConfigEntry<bool> EnableBunnyhopping = new(ConfigSide.Both, "PlayerMovement", nameof(EnableBunnyhopping), () => true);

		public static float DefaultBoost => 0.825f;

		public float Boost { get; set; }

		public override void ResetEffects()
		{
			Boost = DefaultBoost;
		}

		public override void PostItemCheck()
		{
			if (!EnableBunnyhopping) {
				return;
			}

			bool onGround = Player.OnGround();
			bool wasOnGround = Player.WasOnGround();

			if (!onGround && wasOnGround && Player.velocity.Y < 0f) {
				float boost = Boost;

				if (Player.TryGetModPlayer(out PlayerDodgerolls dodgerolls) && dodgerolls.IsDodging) {
					boost += 0.5f;

					if (!Main.dedServ) {
						var playerCenter = Player.Center;
						var entitySource = Player.GetSource_FromThis();

						for (int i = 0; i < 3; i++) {
							var position = playerCenter + new Vector2(Main.rand.NextFloat(-4f, 4f), 0f);
							var velocity = new Vector2((i - 1) * 0.9f, -0.1f);

							Gore.NewGorePerfect(entitySource, position, velocity, GoreID.Smoke1 + i);
						}

						SoundEngine.PlaySound(SoundID.DoubleJump, playerCenter);
					}
				}

				Player.velocity.X += boost * Player.KeyDirection();
			}
		}
	}
}
