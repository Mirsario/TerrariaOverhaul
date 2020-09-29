using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerBunnyhopping : OverhaulPlayer
	{
		public static float DefaultBoost => 0.75f;

		public float boost;

		public override void ResetEffects()
		{
			boost = DefaultBoost;

			player.autoJump = true;
		}

		public override void PostItemCheck()
		{
			bool onGround = player.OnGround();
			bool wasOnGround = player.WasOnGround();

			if(!onGround && wasOnGround && player.velocity.Y < 0f) {
				player.velocity.X += player.controlLeft ? -boost : player.controlRight ? boost : 0f;
			}
		}
	}
}
