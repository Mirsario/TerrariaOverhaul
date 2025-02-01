// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Bosses;

public sealed class EyeOfCthulhuRework : GlobalNPC
{
	private ref struct MappedAI(NPC npc)
	{
		public ref float State = ref npc.ai[0];
		public ref float Timer = ref npc.ai[1];
		public ref float Unknown1 = ref npc.ai[2];
		public ref float Unknown2 = ref npc.ai[3];
	}

	public static readonly ConfigEntry<bool> EnableEyeOfCthulhuEffects = new(ConfigSide.ClientOnly, true, "Bosses");

	//private (float ai0, float ai1, float ai2, float ai3) oldAI;

	public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
	{
		return npc.type is NPCID.EyeofCthulhu;
	}

	public override void SetDefaults(NPC npc)
	{
		if (!EnableEyeOfCthulhuEffects) {
			return;
		}

		npc.HitSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/PainedScreech", 3) {
			Volume = 0.42f,
			PitchVariance = 0.225f,
			Identifier = "EyeOfCthulhu_Pain",
			SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
			MaxInstances = 2,
		};
		npc.DeathSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/PainedScreechSplatter", 3) {
			Volume = 1.00f,
			Pitch = -0.25f,
			Identifier = "EyeOfCthulhu_Death",
			SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
			MaxInstances = 1,
		};

		if (!Main.dedServ && npc.TryGetGlobalNPC(out NpcAudioEffects audioEffects)) {
			audioEffects.Data = new NpcAudioEffects.EffectData {
				// Movement
				MovementSoundVelocityPitching = (2.5f, 10f, -0.50f, 0.50f),
				MovementSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/FleshLoopChaotic") {
					Volume = 0.4f,
					IsLooped = true,
					Identifier = "EyeOfCthulhu_Movement",
					SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				},
			};

			TileSoundOcclusion.SetEnabledForSoundStyle(in audioEffects.Data.ApproachSound, false);
		}
	}

	public override void AI(NPC npc)
	{
		var ai = new MappedAI(npc);

		// If transforming.
		if (ai.State == 1) {
			const int TransformationLength = 99;

			CameraCurios.Create(npc.Center, new() {
				Weight = 2.00f,
				Zoom = +0.5f,
				Range = new(Min: 512f, Max: 1536f, Exponent: 2f),
				LengthInSeconds = 0.10f,
				FadeInLength = 0.25f,
				FadeOutLength = 1.5f,
				UniqueId = "BossTransformation",
			});

			ScreenShakeSystem.New(new() {
				Power = ai.Timer >= TransformationLength ? 1f : 0.4f,
				Range = 1280f,
				LengthInSeconds = ai.Timer >= TransformationLength ? 1f : 0.1f,
				UniqueId = "BossTransformation",
			}, npc.Center);
		}
	}
}

