using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCAttackCooldowns : GlobalNPC
	{
		public int AttackCooldown { get; private set; }
		public bool ShowDamagedEffect { get; private set; }

		private Color defaultColor;

		public override bool InstancePerEntity => true;

		public override bool PreAI(NPC npc)
		{
			if(AttackCooldown > 0) {
				AttackCooldown--;

				if(AttackCooldown == 0 && ShowDamagedEffect) {
					if(defaultColor != null) {
						npc.color = defaultColor;
					}

					ShowDamagedEffect = false;
				}
			}

			return true;
		}
		
		public override bool? CanHitNPC(NPC npc, NPC target)
			=> AttackCooldown > 0 ? false : (bool?)null;
		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
			=> AttackCooldown <= 0;

		public void SetAttackCooldown(NPC npc, int ticks, bool isDamage)
		{
			if(isDamage && !ShowDamagedEffect) {
				defaultColor = npc.color;
				npc.color = Color.Lerp(npc.color, Color.IndianRed, 0.5f);
				ShowDamagedEffect |= isDamage;
			}

			AttackCooldown = Math.Max(ticks, AttackCooldown);
		}
	}
}
