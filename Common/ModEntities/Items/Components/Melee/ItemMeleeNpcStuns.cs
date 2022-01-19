using Terraria;
using TerrariaOverhaul.Common.ModEntities.NPCs;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components.Melee
{
	public sealed class ItemMeleeNpcStuns : ItemComponent
	{
		public override void OnHitNPC(Item item, Player player, NPC target, int damage, float knockback, bool crit)
		{
			if (Enabled) {
				target.GetGlobalNPC<NPCAttackCooldowns>().SetAttackCooldown(target, player.itemAnimationMax, true);
			}
		}
	}
}
