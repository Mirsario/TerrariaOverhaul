// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.Bosses;

internal sealed class EaterOfWorldsRework : GlobalNPC
{
	public static readonly ConfigEntry<bool> EnableEaterOfWorldsEffects = new(ConfigSide.ClientOnly, true, "Bosses");

	public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
	{
		return npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;
	}

	public override void SetDefaults(NPC npc)
	{
		if (!EnableEaterOfWorldsEffects) {
			return;
		}

		npc.HitSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/PainedScreech", 3) {
			Volume = 0.42f,
			PitchVariance = 0.225f,
			Identifier = "EaterOfWorlds_Pain",
			SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
			MaxInstances = 3,
		};
		npc.DeathSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/PainedScreechSplatter", 3) {
			Volume = 0.60f,
			PitchVariance = 0.225f,
			Identifier = "EaterOfWorlds_Death",
			SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
			MaxInstances = 2,
		};

		var wormPart = npc.type switch {
			NPCID.EaterofWorldsHead => NpcWormEffects.PartType.Head,
			NPCID.EaterofWorldsTail => NpcWormEffects.PartType.Tail,
			_ => NpcWormEffects.PartType.Body,
		};

		if (wormPart == NpcWormEffects.PartType.Head && npc.TryGetGlobalNPC(out NpcAudioEffects audioEffects)) {
			audioEffects.Data = new NpcAudioEffects.EffectData {
				// Approach
				ApproachDistance = 384f,
				ApproachSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/LoudScreech", 3) {
					Volume = 0.85f,
					PitchVariance = 0.25f,
					Identifier = "EaterOfWorlds_VoiceImportant",
					MaxInstances = 2,
					SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				},
				ApproachScreenShake = new ScreenShake {
					Power = 0.5f,
					Range = 1384f,
					LengthInSeconds = 2.0f,
					UniqueId = "EaterOfWorlds_Approach",
				},
				// Movement
				MovementSoundVelocityPitching = (2.5f, 10f, -0.50f, 0.50f),
				MovementSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/FleshLoopSoft") {
					Volume = 0.525f,
					IsLooped = true,
					Identifier = "EaterOfWorlds_Movement",
				},
				// Random
				RandomSoundCooldown = (2 * TimeSystem.LogicFramerate, 9 * TimeSystem.LogicFramerate),
				RandomSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/GutteralSpeech", 3) {
					Volume = 0.50f,
					PitchVariance = 0.30f,
					Identifier = "EaterOfWorlds_Voice",
					MaxInstances = 2,
					SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				},
			};

			TileSoundOcclusion.SetEnabledForSoundStyle(in audioEffects.Data.ApproachSound, false);
		}

		if (npc.TryGetGlobalNPC(out NpcWormEffects wormEffects)) {
			wormEffects.Data = new NpcWormEffects.EffectData {
				Type = wormPart,
				SurfacingScreenShake = new ScreenShake {
					Power = 0.40f,
					Range = 1024f,
					LengthInSeconds = 0.5f,
					UniqueId = $"EaterOfWorlds_{npc.whoAmI}",
				},
				DebrisStyle = new() {
					RangeInPixels = 64f,
				},
			};
		}
	}

	// Nobody wants to see a second worm made out of healthbars.
	public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
	{
		return false;
	}
}
