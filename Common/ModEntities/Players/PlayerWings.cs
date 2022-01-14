using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerWings : ModPlayer
	{
		public int noWingsTime;

		public override void PreUpdate()
		{
			// No Wings Time
			if (noWingsTime > 0) {
				Player.wingsLogic = 0;
				noWingsTime--;
			}
		}
	}
}
