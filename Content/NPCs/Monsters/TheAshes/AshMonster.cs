using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.Items.Materials;

namespace TerrariaOverhaul.Content.NPCs.Monsters.TheAshes
{
	public abstract class AshMonster : NPCBase
	{
		protected abstract int BaseNPC { get; }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ash Clot");
		}
		public override void SetDefaults()
		{
			NPC.CloneDefaults(BaseNPC);

			//Sound.
			NPC.HitSound = SoundID.NPCHit18;
			NPC.DeathSound = SoundID.NPCDeath21;
			//Buffs.
			NPC.buffImmune[BuffID.Bleeding] = true;
			NPC.buffImmune[BuffID.Poisoned] = true;
			NPC.buffImmune[BuffID.OnFire] = true;
			NPC.buffImmune[BuffID.Venom] = true;
			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.BrokenArmor] = true;
			NPC.buffImmune[BuffID.Ichor] = true;
			NPC.buffImmune[BuffID.CursedInferno] = true;
			//Animation.
			AnimationType = BaseNPC;
			Main.npcFrameCount[Type] = Main.npcFrameCount[BaseNPC];

			//OverhaulNPC.goreInfos.AddIfNeedTo(npc.type,() => new NPCGoreInfo(npc,bloodColor:Color.Transparent,goreType:""));
		}
		public override void AI()
		{
			if(!Main.dedServ) {
				//Slight glow in the dark, due to the eye.
				Lighting.AddLight(NPC.Top, new Vector3(1f, 0.75f, 0f) * 0.15f);
			}
		}
		public override void HitEffect(int hitDirection, double damage)
		{
			int amount = NPC.life <= 0 ? 50 : (int)damage;

			for(int i = 0; i < amount; i++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, 54, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.Common(ItemID.AshBlock, minimumDropped: 5, maximumDropped: 10));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Charcoal>(), minimumDropped: 1, maximumDropped: 3));
		}
		/*public override void PostDraw(SpriteBatch sb, Color drawColor) //TODO: Reimplement this when tML simplifies glowmasks
		{
			var tex = TextureSystem.GetTexture2D("NPCs/AshSlime_Glow");
			var effects = npc.spriteDirection==1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			var origin = new Vector2(tex.Width/2,tex.Height/Main.npcFrameCount[npc.type]/2);
			//origin.Y -= 4;
			var position = npc.Center-Main.screenPosition;
			sb.Draw(tex,position,npc.frame,OverhaulUtils.colorWhite,npc.rotation,origin,npc.scale,effects,0f);
		}*/

		/*protected void OverhaulInit()
		{
			this.SetTag(NPCTags.AutoBloodColor);
		}*/
	}
}
