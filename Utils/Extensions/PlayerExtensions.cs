using Terraria;

namespace TerrariaOverhaul.Utils.Extensions
{
	public static class PlayerExtensions
	{
		public static bool OnGround(this Player player) => player.velocity.Y==0f;
		public static bool WasOnGround(this Player player) => player.oldVelocity.Y==0f;

		public static int KeyDirection(this Player player)
			=> player.controlLeft ? -1 : player.controlRight ? 1 : 0;
	}
}
