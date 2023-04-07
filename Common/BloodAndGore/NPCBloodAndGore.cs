using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.SimpleEntities;
using TerrariaOverhaul.Core.SimpleEntities;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public class NPCBloodAndGore : GlobalNPC
{
	private static int disableNonBloodEffectSubscriptions;
	private static int disableReplacementsSubscriptions;

	public int LastHitBloodAmount { get; private set; }

	public override bool InstancePerEntity => true;

	public override void Load()
	{
		// Disable blood dust replacement during projectile AI.
		On_Projectile.AI += (orig, proj) => {
			disableReplacementsSubscriptions++;

			try {
				orig(proj);
			}
			finally {
				disableReplacementsSubscriptions--;
			}
		};

		// Replace specific dusts with new blood particles.
		On_Dust.NewDust += (orig, position, width, height, type, speedX, speedY, alpha, color, scale) => {
			bool skipOverride = disableReplacementsSubscriptions > 0;

			if (!skipOverride) {
				skipOverride = !ChildSafety.Disabled && type >= 0 && type <= ChildSafety.SafeDust.Length && !ChildSafety.SafeDust[type];
			}

			if (skipOverride) {
				return orig(position, width, height, type, speedX, speedY, alpha, color, scale);
			}

			void SpawnParticles(Color usedColor) => SpawnNewBlood(
				position + new Vector2(Main.rand.NextFloat(width), Main.rand.NextFloat(height)),
				new Vector2(speedX, speedY) * TimeSystem.LogicFramerate,
				usedColor
			);

			switch (type) {
				default:
					if (disableNonBloodEffectSubscriptions > 0) {
						break;
					}

					return orig(position, width, height, type, speedX, speedY, alpha, color, scale);
				case DustID.Blood:
					SpawnParticles(Color.DarkRed);
					break;
				case DustID.GreenBlood:
					SpawnParticles(Color.Green);
					break;
				case DustID.CorruptGibs:
					SpawnParticles(new Color(94, 104, 17));
					break;
				case DustID.t_Slime:
					SpawnParticles(color);
					break;
			}

			return Main.maxDust;
		};

		// Record and save blood information onto gores spawned during HitEffect.
		On_NPC.HitEffect += (orig, npc, hitDirection, dmg) => {
			// Ignore contexts where we only want blood to spawn.
			if (disableNonBloodEffectSubscriptions > 0 || !npc.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore)) {
				orig(npc, hitDirection, dmg);

				return;
			}

			List<Color>? bloodColors = null;
			
			var spawnedGores = GoreSystem.InvokeWithGoreSpawnRecording(() => {
				bloodColors = BloodParticle.RecordBloodColors(() => {
					orig(npc, hitDirection, dmg);
				});
			});

			if (bloodColors == null) {
				return;
			}

			npcBloodAndGore.LastHitBloodAmount = bloodColors.Count;

			if (spawnedGores.Count == 0 || bloodColors.Count == 0) {
				return;
			}

			// Enumerate the spawned gores, and register blood information to them.
			var bloodColor = bloodColors[0]; //TODO: Do something smarter?
			bool onFire = npc.onFire;

			foreach (var (gore, _) in spawnedGores) {
				if (gore is OverhaulGore goreExt) {
					if (!ChildSafety.SafeGore[gore.type]) {
						goreExt.BleedColor = bloodColor;
					}
					
					goreExt.OnFire = onFire;
				}
			}
		};
	}

	//TODO: Using HitEffect was a bad idea in general. If possible, it's better to try to simulate its particle results instead.
	public static void SpawnBloodWithHitEffect(NPC npc, int direction, int damage)
	{
		int? lifeToRestore = null;

		if (npc.life <= 0) {
			lifeToRestore = npc.life;
			npc.life = 1;
		}

		disableNonBloodEffectSubscriptions++;

		try {
			GoreSystem.InvokeWithGoreSpawnDisabled(() => NPCLoader.HitEffect(npc, direction, damage));
		}
		finally {
			disableNonBloodEffectSubscriptions--;

			if (lifeToRestore.HasValue) {
				npc.life = lifeToRestore.Value;
			}
		}
	}

	public static void Bleed(NPC npc, int amount)
	{
		for (int i = 0; i < amount; i++) {
			SpawnBloodWithHitEffect(npc, npc.direction, 1);
		}
	}

	private static void SpawnNewBlood(Vector2 position, Vector2 velocity, Color color)
	{
		SimpleEntity.Instantiate<BloodParticle>(p => {
			p.position = position;
			p.velocity = velocity * (Main.rand.NextVector2Circular(1f, 1f) + Vector2.One) * 0.5f;

			float intensity = Main.rand.Next(3) switch {
				2 => 0.7f,
				1 => 0.85f,
				_ => 1f,
			};

			color.R = (byte)(color.R * intensity);
			color.G = (byte)(color.G * intensity);
			color.B = (byte)(color.B * intensity);

			p.color = color;
		});
	}
}
