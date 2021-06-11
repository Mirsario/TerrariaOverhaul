namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerInput : PlayerBase
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
