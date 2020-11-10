using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
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
			npc.CloneDefaults(BaseNPC);

			//Sound.
			npc.HitSound = SoundID.NPCHit18;
			npc.DeathSound = SoundID.NPCDeath21;
			//Buffs.
			npc.buffImmune[BuffID.Bleeding] = true;
			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.OnFire] = true;
			npc.buffImmune[BuffID.Venom] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.BrokenArmor] = true;
			npc.buffImmune[BuffID.Ichor] = true;
			npc.buffImmune[BuffID.CursedInferno] = true;
			//Animation.
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[BaseNPC];
			animationType = BaseNPC;

			//OverhaulNPC.goreInfos.AddIfNeedTo(npc.type,() => new NPCGoreInfo(npc,bloodColor:Color.Transparent,goreType:""));
		}
		public override void AI()
		{
			if(!Main.dedServ) {
				//Slight glow in the dark, due to the eye.
				Lighting.AddLight(npc.Top, new Vector3(1f, 0.75f, 0f) * 0.15f);
			}
		}
		public override void HitEffect(int hitDirection, double damage)
		{
			int amount = npc.life <= 0 ? 50 : (int)damage;

			for(int i = 0; i < amount; i++) {
				Dust.NewDust(npc.position, npc.width, npc.height, 54, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
			}
		}
		public override void NPCLoot() //TODO: Use loot tables when tML implements their use.
		{
			Item.NewItem(npc.getRect(), ItemID.AshBlock, Main.rand.Next(5, 10));
			Item.NewItem(npc.getRect(), ModContent.ItemType<Charcoal>(), Main.rand.Next(1, 3));
		}
		public override void PostDraw(SpriteBatch sb, Color drawColor) //TODO: Reimplement this when tML simplifies glowmasks
		{
			/*var tex = TextureSystem.GetTexture2D("NPCs/AshSlime_Glow");
			var effects = npc.spriteDirection==1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			var origin = new Vector2(tex.Width/2,tex.Height/Main.npcFrameCount[npc.type]/2);
			//origin.Y -= 4;
			var position = npc.Center-Main.screenPosition;
			sb.Draw(tex,position,npc.frame,OverhaulUtils.colorWhite,npc.rotation,origin,npc.scale,effects,0f);*/
		}

		/*protected void OverhaulInit()
		{
			this.SetTag(NPCTags.AutoBloodColor);
		}*/
	}
}
