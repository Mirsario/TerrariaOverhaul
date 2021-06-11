using TerrariaOverhaul.Core.Systems.Configuration;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerBunnyhopping : PlayerBase
	{
		public static readonly ConfigEntry<bool> EnableBunnyhopping = new(ConfigSide.ClientOnly, "PlayerMovement", nameof(EnableBunnyhopping), () => true);

		public static float DefaultBoost => 0.8f;

		public float boost;

		public override void ResetEffects()
		{
			boost = DefaultBoost;

			Player.autoJump = true;
		}

		public override void PostItemCheck()
		{
			if(!EnableBunnyhopping) {
				return;
			}

			bool onGround = Player.OnGround();
			bool wasOnGround = Player.WasOnGround();

			if(!onGround && wasOnGround && Player.velocity.Y < 0f) {
				Player.velocity.X += boost * Player.KeyDirection();
			}
		}
	}
}
