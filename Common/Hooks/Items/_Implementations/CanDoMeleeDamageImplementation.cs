using Terraria;
using Terraria.ModLoader;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanDoMeleeDamage;

namespace TerrariaOverhaul.Common.Hooks.Items;

internal sealed class CanDoMeleeDamageImplementation : GlobalItem
{
	public override void Load()
	{
		On_Player.ItemCheck_MeleeHitNPCs += (orig, player, item, itemRectangle, originalDamage, knockback) => {
			if (Hook.Invoke(item, player)) {
				orig(player, item, itemRectangle, originalDamage, knockback);
			}
		};
	}
}
