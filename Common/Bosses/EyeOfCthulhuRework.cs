// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Bosses;

internal sealed class EyeOfCthulhuRework : GlobalNPC
{
	private ref struct MappedAI(NPC npc)
	{
		public ref float State = ref npc.ai[0];
		public ref float Timer = ref npc.ai[1];
		public ref float Unknown1 = ref npc.ai[2];
		public ref float Unknown2 = ref npc.ai[3];
	}

	public static readonly ConfigEntry<bool> EnableEyeOfCthulhuEffects = new(ConfigSide.ClientOnly, true, "Bosses");

	private (uint Start, uint End) glowFadeOut;

	public override bool InstancePerEntity => true;

	//private (float ai0, float ai1, float ai2, float ai3) oldAI;

	public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
	{
		return npc.type is NPCID.EyeofCthulhu or NPCID.Spazmatism or NPCID.Retinazer;
	}

	public override void SetDefaults(NPC npc)
	{
		if (!EnableEyeOfCthulhuEffects)
			return;

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
		if (Main.dedServ || !EnableEyeOfCthulhuEffects)
			return;

		uint timeInTicks = Main.GameUpdateCount;
		var ai = new MappedAI(npc);

		// If transforming.
		if (ai.State == 1) {
			const int TransformationLength = 99;
			const int GibFrequency = 5;
			const int GibChanceOneIn = 1;
			const int NumGibsPerSplat = 3;
			float progress = MathUtils.Clamp01(ai.Timer / TransformationLength);
			bool isEnd = ai.Timer >= TransformationLength;

			if (isEnd || ((int)ai.Timer % GibFrequency == 0 && Main.rand.NextBool(GibChanceOneIn))) {
				SoundEngine.PlaySound(
					OverhaulGore.GoreBreakSound with { Volume = isEnd ? 1.0f : 0.5f, MaxInstances = 3 },
					npc.Center
				);

				for (int i = 0; i < NumGibsPerSplat; i++) {
					var gore = Gore.NewGorePerfect(npc.GetSource_FromThis(),
						npc.Center + Main.rand.NextVector2Circular(npc.width * 0.5f, npc.height * 0.5f),
						Main.rand.NextVector2Circular(12.5f, 12.5f),
						ModContent.GoreType<GenericGore>(),
						(float)Main.rand.NextFloat(1f, 1.5f)
					);
					if (gore is OverhaulGore oGore) {
						oGore.BleedColor = Color.DarkRed;
					}
				}
			}

			CameraCurios.Create(npc.Center, new() {
				Weight = 0.90f,
				Zoom = +0.5f,
				Range = new(Min: 256f, Max: 900f, Exponent: 3f),
				LengthInSeconds = 0.10f,
				FadeInLength = 0.25f,
				FadeOutLength = 1.5f,
				UniqueId = "BossTransformation",
			});

			ScreenShakeSystem.New(new() {
				Power = isEnd ? 1f : 0.4f,
				Range = 1280f,
				LengthInSeconds = isEnd ? 1f : 0.1f,
				UniqueId = "BossTransformation",
			}, npc.Center);

			// Start or restart glow that lasts a little bit past the transformation animation.
			const int GlowFadeOutLength = 90;
			glowFadeOut = (timeInTicks, timeInTicks + GlowFadeOutLength);
		}

		// Glow highlight.
		if (glowFadeOut.End > timeInTicks) {
			float progress = glowFadeOut.Start >= timeInTicks ? 0f : MathUtils.Clamp01((timeInTicks - glowFadeOut.Start) / (float)(glowFadeOut.End - glowFadeOut.Start));
			Lighting.AddLight(npc.Center, new Vector3(1.00f, 0.25f, 0.25f) * (1f - progress));
		}
	}
}

