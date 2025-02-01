// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public class NPCBloodAndGore : GlobalNPC
{
	public static readonly ConfigEntry<bool> EnableAdvancedParticles = new(ConfigSide.ClientOnly, true, "BloodAndGore");

	private static Counter disableVanillaParticlesCounter;
	private static Counter disableReplacementsCounter;

	public int LastHitBloodAmount { get; private set; }

	public override bool InstancePerEntity => true;

	public override void Load()
	{
		// Disable blood dust replacement during projectile AI.
		On_Projectile.AI += (orig, proj) => {
			using (DisableParticleReplacements()) {
				orig(proj);
			}
		};

		On_Dust.NewDust += NewDustDetour;

		On_NPC.HitEffect_HitInfo += HitEffectDetour;
	}

	public static Counter.Handle DisableParticleReplacements()
		=> disableReplacementsCounter.Increase();

	public static Counter.Handle DisableVanillaParticles()
		=> disableVanillaParticlesCounter.Increase();

	//TODO: Using HitEffect was a bad idea in general. If possible, it's better to try to simulate its particle results instead.
	public static void SpawnBloodWithHitEffect(NPC npc, int direction, int damage)
	{
		int? lifeToRestore = null;

		if (npc.life <= 0) {
			lifeToRestore = npc.life;
			npc.life = 1;
		}

		using (DisableVanillaParticles()) {
			using (GoreSystem.DisableGoreSpawn()) {
				try {
					NPCLoader.HitEffect(npc, new NPC.HitInfo {
						Damage = damage,
						HitDirection = direction,
					});
				}
				finally {
					if (lifeToRestore.HasValue) {
						npc.life = lifeToRestore.Value;
					}
				}
			}
		}
	}

	public static void Bleed(NPC npc, int amount)
	{
		for (int i = 0; i < amount; i++) {
			SpawnBloodWithHitEffect(npc, npc.direction, 1);
		}
	}

	// Replace specific dusts with new blood particles.
	private static int NewDustDetour(On_Dust.orig_NewDust orig, Vector2 position, int width, int height, int type, float speedX, float speedY, int alpha, Color color, float scale)
	{
		if (!EnableAdvancedParticles
			|| disableReplacementsCounter.Active
			|| (!ChildSafety.Disabled && type >= 0 && type <= ChildSafety.SafeDust.Length && !ChildSafety.SafeDust[type])) {
			return orig(position, width, height, type, speedX, speedY, alpha, color, scale);
		}

		void SpawnParticles(Color usedColor, bool isViolent)
		{
			Span<ParticleSystem.ParticleData> particleSpan = stackalloc ParticleSystem.ParticleData[1];

			ParticleSystem.ConfigureParticles(
				particleSpan,
				position + new Vector2(Main.rand.NextFloat(width), Main.rand.NextFloat(height)),
				new Vector2(speedX, speedY) * TimeSystem.LogicFramerate,
				usedColor,
				isViolent
			);

			ParticleSystem.SpawnParticles(particleSpan);
		}

		switch (type) {
			default:
				if (disableVanillaParticlesCounter.Active) {
					break;
				}

				return orig(position, width, height, type, speedX, speedY, alpha, color, scale);
			case DustID.Blood:
				SpawnParticles(Color.DarkRed, isViolent: true);
				break;
			case DustID.GreenBlood:
				SpawnParticles(Color.Green, isViolent: true);
				break;
			case DustID.CorruptGibs:
				SpawnParticles(new Color(94, 104, 17), isViolent: true);
				break;
			case DustID.t_Slime:
				SpawnParticles(color, isViolent: false);
				break;
		}

		return Main.maxDust;
	}

	// Record and save blood information onto gores spawned during HitEffect.
	private static void HitEffectDetour(On_NPC.orig_HitEffect_HitInfo orig, NPC npc, NPC.HitInfo hitInfo)
	{
		// Ignore contexts where we only want blood to spawn.
		if (disableVanillaParticlesCounter.Active || !npc.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore)) {
			orig(npc, hitInfo);

			return;
		}

		List<Color>? bloodColors = null;

		var spawnedGores = GoreSystem.InvokeWithGoreSpawnRecording(() => {
			bloodColors = BloodColorRecording.RecordBloodColors(() => {
				orig(npc, hitInfo);
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
	}
}
