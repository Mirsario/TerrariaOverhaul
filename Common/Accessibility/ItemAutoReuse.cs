using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Accessibility
{
	public class ItemAutoReuse : GlobalItem
	{
		public static readonly ConfigEntry<bool> ForceAutoReuse = new(ConfigSide.ClientOnly, "Accessibility", nameof(ForceAutoReuse), () => true);

		public override void SetDefaults(Item item)
		{
			if (!ForceAutoReuse) {
				return;
			}

			if (item.autoReuse || item.channel) {
				return;
			}

			//TODO: Implement through APIs instead.
			if (item.ModItem?.Mod is { Name: "ClickerClass" }) {
				return;
			}

			item.autoReuse = true;
			item.useTime += 2;
			item.useAnimation += 2;
		}
	}
}
