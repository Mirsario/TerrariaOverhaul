using Terraria;
using TerrariaOverhaul.Common.Damage;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemMeleeNpcStuns : ItemComponent
{
	public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
	{
		if (Enabled) {
			target.GetGlobalNPC<NPCAttackCooldowns>().SetAttackCooldown(target, player.itemAnimationMax, true);
		}
	}
}
