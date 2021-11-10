using Microsoft.Xna.Framework;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerAnimations : PlayerBase
	{
		public PlayerFrames? forcedHeadFrame;
		public PlayerFrames? forcedBodyFrame;
		public PlayerFrames? forcedLegFrame;

		public override void PostUpdate()
		{
			void TryForceFrame(ref Rectangle frame, ref PlayerFrames? newFrame)
			{
				if (newFrame.HasValue) {
					frame = newFrame.Value.ToRectangle();

					newFrame = null;
				}
			}

			TryForceFrame(ref Player.headFrame, ref forcedHeadFrame);
			TryForceFrame(ref Player.bodyFrame, ref forcedBodyFrame);
			TryForceFrame(ref Player.legFrame, ref forcedLegFrame);
		}
	}
}
