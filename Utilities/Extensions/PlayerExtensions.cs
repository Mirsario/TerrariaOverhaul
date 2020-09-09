using Terraria;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class PlayerExtensions
	{
		public static bool IsLocal(this Player player) => player.whoAmI==Main.myPlayer;

		public static bool OnGround(this Player player) => player.velocity.Y==0f; //player.GetModPlayer<PlayerMovement>().OnGround;
		public static bool WasOnGround(this Player player) => player.oldVelocity.Y==0f; //player.GetModPlayer<PlayerMovement>().WasOnGround;

		public static int KeyDirection(this Player player) => player.controlLeft ? -1 : player.controlRight ? 1 : 0;
	}
}
