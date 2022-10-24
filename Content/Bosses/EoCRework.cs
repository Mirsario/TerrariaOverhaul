﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AI;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Content.Bosses;

public class EoCRework : ModNPC
{
	public const int WhipAttackTime = 90;

	private int currentFrame;
	private int frameCounter;
	private int shootCount;
	private bool initialized;
	private int phase;
	private List<NPC> tailSegments;

	private uint timeSinceLastUpdate;
	private int spinTime;
	private bool shouldGetStuck;

	public override string BossHeadTexture => "Terraria/Content/Images/NPC_Head_Boss_20";

	public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
	{
		damage = (int)(damage * 0.25f);
	}

	public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
	{
		damage = (int)(damage * 0.25f);
	}

	public override void SetStaticDefaults()
	{
		// TO-DO: Use in-game translation;
		DisplayName.SetDefault("Eye of Cthulhu");
	}

	public override void SetDefaults()
	{
		// Copying some stuff from vanilla EoC for convinience
		NPC.CloneDefaults(NPCID.EyeofCthulhu);
		NPC.aiStyle = -1;
		// Combat
		NPC.damage = 0;
		tailSegments = new List<NPC>();
	}

	public override bool PreAI()
	{
		if (!initialized) {
			initialized = true;

			int latest = NPC.whoAmI;
			for (int i = 0; i < 20; i++) {
				IEntitySource source = NPC.GetSource_FromAI();
				int tailID = NPC.NewNPC(source, (int)NPC.position.X, (int)NPC.position.Y, ModContent.NPCType<EoCTail>());
				NPC tailSegment = Main.npc[tailID];
				tailSegment.realLife = NPC.whoAmI;
				tailSegment.ai[3] = latest;
				latest = tailID;

				tailSegments.Add(tailSegment);
			}

			NPC.TryGetGlobalNPC(out NpcGetComfortableDistance comfortableDistance);
			comfortableDistance?.SetPrefferedDistance(240, 240, 1.5f);
			comfortableDistance?.SetMovementSpeed(2.5f, 1.5f);
		}

		return base.PreAI();
	}

	public override void AI()
	{
		// animation
		if (++frameCounter > 5) {
			if (++currentFrame >= Main.npcFrameCount[NPCID.EyeofCthulhu] / 2 + 3 * phase) {
				currentFrame = 3 * phase;
			}
			frameCounter = 0;
		}

		// Whip attack
		if (spinTime > 0) {
			spinTime--;

			// if missed whip attack 3 times by now, get stuck midway through the attack
			if (shouldGetStuck) {
				if(NPC.rotation <= NPC.GetGlobalNPC<NpcStareAtTargets>().StareAngle + MathHelper.Pi) {
					NPC.rotation += 0.06f;
				}
			} else {
				NPC.rotation += 0.06f;
			}

			// if at least one segment landed a hit, assume every other did as well
			if(tailSegments.Count(x => (x.ModNPC as EoCTail).HasHitAtLeastOne) > 0) {
				tailSegments.ForEach(x => (x.ModNPC as EoCTail).HasHitAtLeastOne = true);
			}

		} else {
			// reset miss related effect
			shouldGetStuck = false;

			// look at the average position of every applicable target
			NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.GetGlobalNPC<NpcStareAtTargets>().StareAngle, 0.15f);
		}

		NPC.TargetClosest();

		// probably quite confusing part cuz it sucks
		if (++timeSinceLastUpdate > 120) {

			// if we aren't stuck, try to adjust it's own position every 4-ish seconds
			if (!shouldGetStuck) {
				NPC.GetGlobalNPC<NpcGetComfortableDistance>().UpdateTargetPosition();
			} else {
				timeSinceLastUpdate = 0;
				return;
			}

			// 1 second after updating our position, try to perform an attack
			if (timeSinceLastUpdate > 180) {
				// if we did more than 3 shots by now, do whip attack
				if (++shootCount > 3) {
					shootCount = 0;
					timeSinceLastUpdate = 0;

					// additional time, basically just a band aid to make sure the tail stays in place after getting "stuck"
					int additional = 0;

					if (NPC.ai[3] >= 3) {
						additional = 550;
						shouldGetStuck = true;
						NPC.ai[3] = 0;
					}

					// time we are going to spin for the whip attack
					spinTime = WhipAttackTime + additional;

					// make sure every segment spins
					foreach (NPC segment in tailSegments) {

						segment.ai[1] = WhipAttackTime + additional;

						if (segment.ModNPC is EoCTail tail) {
							tail.ShouldFreeze = true;
						}
					}
				} else {
					timeSinceLastUpdate = 0;
					// shoot servants;
					IEntitySource source = NPC.GetSource_FromAI();

					for (int i = 0; i < Main.rand.Next(3, 6); i++) {
						int servant = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.ServantofCthulhu);
						var targetPos = NPC.GetTarget().Center + new Vector2(Main.rand.Next(-80, 80), Main.rand.Next(-80, 80));
						var velocity = (targetPos - NPC.Center).SafeNormalize(-Vector2.UnitY) * (float)Main.rand.Next(6, 12);
						Main.npc[servant].velocity = velocity;
					}
				}
			}
		}
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		=> false;

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		// draw eye itself
		var texture = TextureAssets.Npc[NPCID.EyeofCthulhu].Value;
		var frameCount = Main.npcFrameCount[NPCID.EyeofCthulhu];
		var frameHeight = texture.Height / frameCount;
		var sourceRect = new Rectangle(0, frameHeight * currentFrame, texture.Width, frameHeight);
		var origin = new Vector2(texture.Width, frameHeight) * 0.5f;

		Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, sourceRect, drawColor, NPC.rotation - MathHelper.PiOver2, origin, Vector2.One, SpriteEffects.None, 1);
	}
}
