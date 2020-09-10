using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerItems : ModPlayer
	{
		public bool canUseItems;

		public override void PostUpdate() => canUseItems = true;
	}
}
