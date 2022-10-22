using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Music;

public sealed class BossDeathMusicMuting : GlobalNPC
{
	private static readonly Gradient<float> volumeGradient = new(
		(0.00f, 0f),
		(0.75f, 0f),
		(1.00f, 1f)
	);

	public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
	{
		return npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
	}

	public override void HitEffect(NPC npc, int hitDirection, double damage)
	{
		if (npc.life < 0) {
			const float MuteTimeInSeconds = 5f;

			int muteTimeInTicks = (int)(MuteTimeInSeconds * TimeSystem.LogicFramerate);

			AudioEffectsSystem.AddAudioEffectModifier(muteTimeInTicks, nameof(BossDeathMusicMuting), Modifier);
		}
	}

	private static void Modifier(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters musicParameters)
	{
		musicParameters.Volume = MathF.Min(musicParameters.Volume, volumeGradient.GetValue(1f - intensity));
	}
}
