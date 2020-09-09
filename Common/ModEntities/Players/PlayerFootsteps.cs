using Terraria;
using TerrariaOverhaul.Common.Systems.Footsteps;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerFootsteps : OverhaulPlayer
	{
		private byte stepState;

		public override void PreUpdate()
		{

		}
		public override void PostUpdate()
		{

		}
		public override void PostItemCheck()
		{
			if(Main.dedServ) {
				return;
			}

			bool onGround = player.OnGround();
			bool wasOnGround = player.WasOnGround();
			bool forceFootstep = onGround != wasOnGround; 

			if(onGround || forceFootstep) {
				int legFrame = player.legFrame.Y/player.legFrame.Height;

				if(forceFootstep || (stepState==1 && (legFrame==16 || legFrame==17)) || (stepState==0 && (legFrame==9 || legFrame==10))) {
					if(FootstepSystem.Foostep(player)) {
						stepState = stepState==0 ? (byte)1 : (byte)0;
					}
				}
			}
		}
	}
}
