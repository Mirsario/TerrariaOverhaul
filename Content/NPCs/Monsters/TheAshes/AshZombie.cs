using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerrariaOverhaul.Content.NPCs.Monsters.TheAshes
{
	public class AshZombie : AshMonster
	{
		protected override int BaseNPC => NPCID.BloodZombie;

		public override void SetDefaults()
		{
			base.SetDefaults();

			// Combat.
			NPC.damage = 40;
			NPC.knockBackResist = 0.35f;
			NPC.defense = 20;
			NPC.lifeMax = NPC.life = 100;
			// Universal.
			NPC.color = Color.White;
			NPC.alpha = 0;
			NPC.value = 200f;
		}
	}
}
