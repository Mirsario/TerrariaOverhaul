using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Banners
{
	public sealed class ItemBannerRework : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (!BannerReworkSystem.BannerReworkEnabled || !item.consumable || item.createTile <= 0) {
				return;
			}

			string vanillaDescription = Language.GetTextValue("CommonItemTooltip.BannerBonus");
			string overhaulDescription = OverhaulMod.Instance.GetTextValue("Banners.BannerItemDescription");

			foreach (var line in tooltips) {
				string text = line.Text;

				line.Text = line.Text.Replace(vanillaDescription, $"{overhaulDescription}\r\n[c/a5fc8d:");

				if (line.Text != text) {
					line.Text += "]";
				}
			}
		}
	}
}
