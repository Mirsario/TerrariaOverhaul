using Terraria;
using TerrariaOverhaul.Common.Systems.Footsteps;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerFootsteps : PlayerBase
	{
		private const double FootstepCooldown = 0.1;

		private byte stepState;
		private double lastFootstepTime;

		public override void PostItemCheck()
		{
			if(Main.dedServ) {
				return;
			}

			bool onGround = player.OnGround();
			bool wasOnGround = player.WasOnGround();
			bool forceFootstep = onGround != wasOnGround;
			int legFrame = player.legFrame.Y / player.legFrame.Height;

			if(onGround || forceFootstep) {
				if(forceFootstep || (stepState == 1 && (legFrame == 16 || legFrame == 17)) || (stepState == 0 && (legFrame == 9 || legFrame == 10))) {
					double time = TimeSystem.GlobalTime;

					if(time - lastFootstepTime > FootstepCooldown && FootstepSystem.Foostep(player)) {
						stepState = stepState == 0 ? (byte)1 : (byte)0;
						lastFootstepTime = TimeSystem.GlobalTime;
					}
				}
			}

			if(!onGround || legFrame == 0) {
				stepState = 0;
			}
		}
	}
}
