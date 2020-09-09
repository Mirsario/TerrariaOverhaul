using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerJumpBuffering : ModPlayer
	{
		private float jumpKeyBuffer;

		public override void Load()
		{
			On.Terraria.Player.JumpMovement += JumpMovement;
		}
		public override void PostUpdate()
		{
			jumpKeyBuffer = MathUtils.StepTowards(jumpKeyBuffer,0f,TimeSystem.LogicDeltaTime);
		}
		public override void SetControls()
		{
			if(player.controlJump) {
				jumpKeyBuffer = 0.25f;
			}
		}

		private static void JumpMovement(On.Terraria.Player.orig_JumpMovement orig,Player player)
		{
			var modPlayer = player.GetModPlayer<PlayerJumpBuffering>();
			bool forceJump = !player.controlJump && modPlayer.jumpKeyBuffer>0f && player.velocity.Y==0f;
			bool originalReleaseJump = player.releaseJump;

			if(forceJump) {
				modPlayer.jumpKeyBuffer = 0f;

				player.controlJump = true;
				player.releaseJump = true;
			}

			orig(player);

			if(forceJump) {
				player.controlJump = false;
				player.releaseJump = originalReleaseJump;
			}
		}
	}
}
