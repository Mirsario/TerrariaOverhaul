using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerItemUse : ModPlayer
	{
		private bool forceItemUse;
		private int altFunctionUse;

		public override bool PreItemCheck()
		{
			if(forceItemUse) {
				Player.controlUseItem = true;
				Player.altFunctionUse = altFunctionUse;
				Player.itemAnimation = 0;
				Player.itemTime = 0;

				forceItemUse = false;
			}

			return true;
		}

		public void ForceItemUse(int altFunctionUse = 0)
		{
			forceItemUse = true;
			this.altFunctionUse = altFunctionUse;
		}
	}
}
