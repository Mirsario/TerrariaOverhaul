using Terraria.ModLoader;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanDoMeleeDamage;

namespace TerrariaOverhaul.Common.Hooks.Items;

internal sealed class CanDoMeleeDamageImplementation : GlobalItem
{
	public override void Load()
	{
		On.Terraria.Player.ItemCheck_MeleeHitNPCs += (orig, player, item, itemRectangle, originalDamage, knockback) => {
			if (Hook.Invoke(item, player)) {
				orig(player, item, itemRectangle, originalDamage, knockback);
			}
		};
	}
}
