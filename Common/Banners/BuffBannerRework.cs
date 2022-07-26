using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Banners;

public sealed class BuffBannerRework : GlobalBuff
{
	public override void ModifyBuffTip(int type, ref string tip, ref int rare)
	{
		if (BannerReworkSystem.BannerReworkEnabled && type == BuffID.MonsterBanner) {
			tip = OverhaulMod.Instance.GetTextValue("Banners.BannerBuffDescription");
		}
	}
}
