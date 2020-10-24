using Terraria;

namespace TerrariaOverhaul.Utilities.Extensions
{
	public static class PlayerExtensions
	{
		public static bool IsLocal(this Player player) => player.whoAmI == Main.myPlayer;

		public static bool OnGround(this Player player) => player.velocity.Y == 0f; //player.GetModPlayer<PlayerMovement>().OnGround;
		public static bool WasOnGround(this Player player) => player.oldVelocity.Y == 0f; //player.GetModPlayer<PlayerMovement>().WasOnGround;

		public static int KeyDirection(this Player player) => player.controlLeft ? -1 : player.controlRight ? 1 : 0;

		public static void StopGrappling(this Player player, Projectile exceptFor = null)
		{
			for(int i = 0; i < player.grappling.Length; i++) {
				int grapplingHookId = player.grappling[i];

				if(grapplingHookId < 0) {
					continue;
				}

				var grapplingHook = Main.projectile[grapplingHookId];

				if(grapplingHook != null && grapplingHook.active && grapplingHook != exceptFor && grapplingHook.ai[0] == 2f) {
					grapplingHook.Kill();

					player.grappling[i] = -1;
				}
			}
		}
	}
}
