using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Movement
{
	public sealed class PlayerAutoJump : ModPlayer
	{
		public override void ResetEffects()
		{
			Player.autoJump = true;
		}
	}
}
