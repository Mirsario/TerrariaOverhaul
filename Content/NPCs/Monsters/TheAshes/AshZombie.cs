using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerrariaOverhaul.Content.NPCs.Monsters.TheAshes
{
	public class AshZombie : AshMonster
	{
		protected override int BaseNPC => NPCID.BloodZombie;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Walking Ashes");
		}
		public override void SetDefaults()
		{
			base.SetDefaults();

			//Combat.
			npc.damage = 40;
			npc.knockBackResist = 0.35f;
			npc.defense = 20;
			npc.lifeMax = npc.life = 100;
			//Universal.
			npc.color = Color.White;
			npc.alpha = 0;
			npc.value = 200f;
		}
	}
}