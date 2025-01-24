// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.Enemies;

public sealed class EvilMonstersRework : GlobalNPC
{
	public static readonly ConfigEntry<bool> EnableNormalEnemyEffects = new(ConfigSide.ClientOnly, true, "Enemies");

	public static bool[] IsCorruptionMonster = null!;
	public static bool[] IsCrimsonMonster = null!;

	public override void SetStaticDefaults()
	{
		IsCorruptionMonster = NPCID.Sets.Factory.CreateBoolSet([
			NPCID.BigMimicCorruption,
			NPCID.DesertGhoulCorruption,
			NPCID.DevourerHead,
			NPCID.DevourerBody,
			NPCID.DevourerTail,
			NPCID.EaterofSouls,
			NPCID.Corruptor,
			NPCID.CorruptBunny,
			NPCID.CorruptGoldfish,
			NPCID.CorruptPenguin,
			NPCID.CorruptSlime,
			NPCID.PigronCorruption,
			NPCID.SandsharkCorrupt,
			NPCID.SeekerHead,
			NPCID.SeekerBody,
			NPCID.SeekerTail,
			NPCID.Slimer,
		]);
		IsCrimsonMonster = NPCID.Sets.Factory.CreateBoolSet([
			NPCID.BloodCrawler,
			NPCID.BloodCrawlerWall,
			NPCID.BloodFeeder,
			NPCID.BloodJelly,
			NPCID.BloodMummy,
			NPCID.Crimera,
			NPCID.Crimslime,
			NPCID.FaceMonster,
			NPCID.Herpling,
		]);
	}

	public override void SetDefaults(NPC npc)
	{
		if (!EnableNormalEnemyEffects) {
			return;
		}

		if (IsCorruptionMonster?[npc.type] != true && IsCrimsonMonster?[npc.type] != true) {
			return;
		}

		bool humanoid = npc.DeathSound == SoundID.NPCDeath2;
		bool isCrawlingSpider = npc.aiStyle is NPCAIStyleID.Spider;

		if (npc.HitSound == null || npc.HitSound == SoundID.NPCHit1 || npc.HitSound == SoundID.NPCHit20) {
			npc.HitSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/PainedScreech", 3) {
				Volume = 0.25f,
				PitchVariance = 0.225f,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				MaxInstances = 3,
				Identifier = "EvilMonster_Hit",
			};
		}

		if (npc.DeathSound == null || npc.DeathSound == SoundID.NPCDeath1 || npc.DeathSound == SoundID.NPCDeath2 || npc.DeathSound == SoundID.NPCDeath23) {
			npc.DeathSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/PainedScreechSplatter", 3) {
				Volume = 0.45f,
				PitchVariance = 0.225f,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				MaxInstances = 2,
				Identifier = "EvilMonster_Death",
			};
		}

		if (humanoid) {
			npc.HitSound = npc.HitSound.Value with { Pitch = -0.4f };
			npc.DeathSound = npc.DeathSound!.Value with { Pitch = -0.2f };
		}

		if (!humanoid && (npc.realLife < 0 || npc.realLife == npc.whoAmI) && npc.TryGetGlobalNPC(out NpcAudioEffects audioEffects)) {
			audioEffects.Data = new NpcAudioEffects.EffectData {
				// Movement
				MovementSoundVelocityPitching = (2.5f, 10f, -0.30f, 0.20f),
				MovementSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/FleshLoop{(isCrawlingSpider ? "Chaotic" : "Soft")}") {
					Volume = 0.2f,
					IsLooped = true,
					Identifier = "EvilMonster_Movement",
					MaxInstances = 3,
					SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				},
				// Random
				/*
				RandomSoundCooldown = (2 * TimeSystem.LogicFramerate, 9 * TimeSystem.LogicFramerate),
				RandomSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/GutteralSpeech2") {
					Volume = 0.30f,
					Pitch = 0.20f,
					PitchVariance = 0.50f,
					MaxInstances = 2,
					SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
					Identifier = "EvilMonster_Speech",
				},
				*/
			};
		}
	}
}
