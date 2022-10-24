using System.Collections.Generic;
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

	private readonly List<EoCTail> tailSegments = new();

	private int currentFrame;
	private int frameCounter;
	private int shootCount;
	private int phase;
	private int spinTime;
	private uint timeSinceLastUpdate;
	private bool shouldGetStuck;

	public override string BossHeadTexture => "Terraria/Content/Images/NPC_Head_Boss_20";

	public override void SetStaticDefaults()
	{
		// TO-DO: Use in-game translation;
		DisplayName.SetDefault("Eye of Cthulhu");
	}

	public override void SetDefaults()
	{
		// Common
		NPC.boss = true;
		NPC.value = 30000f;
		NPC.npcSlots = 5f;
		NPC.aiStyle = -1;
		NPC.SpawnWithHigherTime(30);
		// Physical properties
		NPC.width = 100;
		NPC.height = 110;
		NPC.noGravity = true;
		NPC.noTileCollide = true;
		// Combat
		NPC.damage = 0; // No contact damage.
		NPC.defense = 12;
		NPC.lifeMax = 2800;
		NPC.knockBackResist = 0f;
		// Audio
		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
	}

	public override void OnSpawn(IEntitySource source)
	{
		int latest = NPC.whoAmI;
		var tailSource = NPC.GetSource_FromThis();

		//TODO: Handle running into the NPC limit by self-destructing?
		for (int i = 0; i < 20; i++) {
			NPC tailNPC = NPC.NewNPCDirect(tailSource, (int)NPC.position.X, (int)NPC.position.Y, ModContent.NPCType<EoCTail>());

			if (tailNPC.ModNPC is not EoCTail tail || tailNPC.whoAmI is < 0 or >= Main.maxNPCs) {
				continue;
			}

			tailNPC.realLife = NPC.whoAmI;
			tailNPC.ai[3] = latest;
			latest = tailNPC.whoAmI;

			tailSegments.Add(tail);
		}

		NPC.TryGetGlobalNPC(out NpcGetComfortableDistance comfortableDistance);

		comfortableDistance?.SetPreferredDistance(new Vector2(240f, 240f), 1.5f);
		comfortableDistance?.SetMovementSpeed(2.5f, 1.5f);
	}

	public override void AI()
	{
		// Animation
		if (++frameCounter > 5) {
			if (++currentFrame >= Main.npcFrameCount[NPCID.EyeofCthulhu] / 2 + 3 * phase) {
				currentFrame = 3 * phase;
			}

			frameCounter = 0;
		}

		// Whip attack
		if (spinTime > 0) {
			spinTime--;

			// If missed whip attack 3 times by now, get stuck midway through the attack
			if (shouldGetStuck) {
				if (NPC.rotation <= NPC.GetGlobalNPC<NpcStareAtTargets>().StareAngle + MathHelper.Pi) {
					NPC.rotation += 0.06f;
				}
			} else {
				NPC.rotation += 0.06f;
			}

			// If at least one segment landed a hit, assume every other did as well
			if (tailSegments.Any(t => t.HasHitAtLeastOne)) {
				tailSegments.ForEach(t => t.HasHitAtLeastOne = true);
			}
		} else {
			// Reset miss related effect
			shouldGetStuck = false;

			// Look at the average position of every applicable target
			NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.GetGlobalNPC<NpcStareAtTargets>().StareAngle, 0.15f);
		}

		NPC.TargetClosest();

		// Probably quite confusing part cuz it sucks
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
				// If we did more than 3 shots by now, do whip attack
				if (++shootCount > 3) {
					shootCount = 0;
					timeSinceLastUpdate = 0;

					// Additional time, basically just a band aid to make sure the tail stays in place after getting "stuck"
					int additional = 0;

					if (NPC.ai[3] >= 3) {
						additional = 550;
						shouldGetStuck = true;
						NPC.ai[3] = 0;
					}

					// time we are going to spin for the whip attack
					spinTime = WhipAttackTime + additional;

					// make sure every segment spins
					foreach (EoCTail tail in tailSegments) {
						tail.NPC.ai[1] = WhipAttackTime + additional;
						tail.ShouldFreeze = true;
					}
				} else {
					timeSinceLastUpdate = 0;
					// shoot servants;
					IEntitySource source = NPC.GetSource_FromAI();
					var target = NPC.GetTarget();

					if (target != null) {
						var targetPosition = target.Center;

						for (int i = 0; i < Main.rand.Next(3, 6); i++) {
							int servant = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.ServantofCthulhu);
							var offsetTargetPosition = targetPosition + Main.rand.NextVector2Circular(80f, 80f);
							var velocity = (offsetTargetPosition - NPC.Center).SafeNormalize(-Vector2.UnitY) * Main.rand.NextFloat(6f, 12f);

							Main.npc[servant].velocity = velocity;
						}
					}
				}
			}
		}
	}

	public override void ModifyHitByItem(Player player, Item item, ref int damage, ref float knockback, ref bool crit)
	{
		damage = (int)(damage * 0.25f);
	}

	public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
	{
		damage = (int)(damage * 0.25f);
	}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		=> false;

	public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		// draw eye itself
		var texture = TextureAssets.Npc[NPCID.EyeofCthulhu].Value;
		int frameCount = Main.npcFrameCount[NPCID.EyeofCthulhu];
		int frameHeight = texture.Height / frameCount;
		var sourceRect = new Rectangle(0, frameHeight * currentFrame, texture.Width, frameHeight);
		var origin = new Vector2(texture.Width, frameHeight) * 0.5f;

		Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, sourceRect, drawColor, NPC.rotation - MathHelper.PiOver2, origin, Vector2.One, SpriteEffects.None, 1);
	}
}
