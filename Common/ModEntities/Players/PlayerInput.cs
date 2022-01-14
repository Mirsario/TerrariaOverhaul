using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerInput : ModPlayer
	{
		public bool controlHookPrev;
		public bool controlJumpPrev;

		//TODO: Sync.

		public override void SetControls()
		{
			/*controlHookPrev = controlHook;
			controlHook = player.controlHook;*/
		}

		public override void PreUpdate()
		{
			controlJumpPrev = Player.controlJump;
			controlHookPrev = Player.controlHook;
		}
	}
}
