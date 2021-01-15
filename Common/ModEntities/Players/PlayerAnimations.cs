using Microsoft.Xna.Framework;
using TerrariaOverhaul.Utilities.Enums;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerAnimations : PlayerBase
	{
		private const int PlayerSheetWidth = 40;
		private const int PlayerSheetHeight = 56;

		public PlayerFrames? forcedHeadFrame;
		public PlayerFrames? forcedBodyFrame;
		public PlayerFrames? forcedLegFrame;

		public override void PostUpdate()
		{
			void TryForceFrame(ref Rectangle frame, ref PlayerFrames? newFrame)
			{
				if(newFrame.HasValue) {
					frame = new Rectangle(0, PlayerSheetHeight * (int)newFrame.Value, PlayerSheetWidth, PlayerSheetHeight);

					newFrame = null;
				}
			}

			TryForceFrame(ref Player.headFrame, ref forcedHeadFrame);
			TryForceFrame(ref Player.bodyFrame, ref forcedBodyFrame);
			TryForceFrame(ref Player.legFrame, ref forcedLegFrame);
		}
	}
}
