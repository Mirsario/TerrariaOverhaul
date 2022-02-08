using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerInput : ModPlayer
	{
		public bool ControlHookPrev { get; private set; }
		public bool ControlJumpPrev { get; private set; }

		//TODO: Sync.

		public override void SetControls()
		{
			/*controlHookPrev = controlHook;
			controlHook = player.controlHook;*/
		}

		public override void PreUpdate()
		{
			ControlJumpPrev = Player.controlJump;
			ControlHookPrev = Player.controlHook;
		}
	}
}
