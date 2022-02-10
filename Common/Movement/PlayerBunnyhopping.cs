using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Movement
{
	public sealed class PlayerBunnyhopping : ModPlayer
	{
		public static readonly ConfigEntry<bool> EnableBunnyhopping = new(ConfigSide.Both, "PlayerMovement", nameof(EnableBunnyhopping), () => true);

		public static float DefaultBoost => 0.8f;

		public float Boost { get; set; }

		public override void ResetEffects()
		{
			Boost = DefaultBoost;

			Player.autoJump = true;
		}

		public override void PostItemCheck()
		{
			if (!EnableBunnyhopping) {
				return;
			}

			bool onGround = Player.OnGround();
			bool wasOnGround = Player.WasOnGround();

			if (!onGround && wasOnGround && Player.velocity.Y < 0f) {
				Player.velocity.X += Boost * Player.KeyDirection();
			}
		}
	}
}
