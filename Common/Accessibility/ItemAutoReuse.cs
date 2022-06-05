using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Accessibility
{
	public class ItemAutoReuse : GlobalItem
	{
		public override void SetDefaults(Item item)
		{
			if (!item.autoReuse && !item.channel) {
				item.autoReuse = true;
				item.useTime += 2;
				item.useAnimation += 2;
			}
		}
	}
}
