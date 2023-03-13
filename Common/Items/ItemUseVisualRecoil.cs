using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.PlayerEffects;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Items;

[Autoload(Side = ModSide.Client)]
public sealed class ItemUseVisualRecoil : ItemComponent
{
	public float Power { get; set; }

	public override void OnEnabled(Item item)
	{
		int timer = Math.Max(item.useTime, item.useAnimation);

		Power = timer / 1.5f;
	}

	public override void SetDefaults(Item item)
	{
		static bool CheckItem(Item item)
		{
			// Only apply to items that fire projectiles.
			if (item.shoot <= ProjectileID.None) {
				return false;
			}

			// Ignore summons and anything that gives buffs.
			if (item.buffType > 0) {
				return false;
			}

			// Ignore channeled items
			if (item.channel) {
				return false;
			}

			// Ignore drills, chainsaws, and jackhammers.
			if (item.pick > 0 || item.axe > 0 || item.hammer > 0) {
				return false;
			}

			return true;
		}

		if (!Enabled && CheckItem(ContentSampleUtils.TryGetItem(item.type, out var baseItem) ? baseItem : item)) {
			SetEnabled(item, true);
		}
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (Enabled && Power != 0f) {
			player.GetModPlayer<PlayerHoldOutAnimation>().VisualRecoil += Power;
		}

		return base.UseItem(item, player);
	}
}
