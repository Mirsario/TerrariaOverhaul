using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerWings : ModPlayer
	{
		public Timer WingsCooldown { get; set; }

		public override void PreUpdate()
		{
			// No Wings Time
			if (WingsCooldown.Active) {
				Player.wingsLogic = 0;
			}
		}
	}
}
