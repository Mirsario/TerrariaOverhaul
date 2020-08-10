using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerrariaOverhaul.Content.NPCs.Monsters.TheAshes
{
	public class AshSlime : OverhaulNPC
	{
		private const int BaseNPC = NPCID.BlueSlime;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ash Clot");
		}
		public override void SetDefaults()
		{
			base.SetDefaults();

			//Combat.
			npc.damage = 50;
			npc.knockBackResist = 0.4f;
			npc.defense = 20;
			npc.lifeMax = npc.life = 75;
			//Universal.
			npc.width = 40;
			npc.height = 27;
			npc.color = Color.White;
			npc.alpha = 0;
			npc.value = 200f;
		}
	}
}