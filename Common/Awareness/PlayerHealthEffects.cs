// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Awareness;

[Autoload(Side = ModSide.Client)]
public sealed class PlayerHealthEffects : ModPlayer
{
	public static readonly SoundStyle LowHealthSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/LowHealthLoop");
	public static readonly Gradient<float> LowHealthEffectGradient = new(
		(0f, 1f),
		(30f, 1f),
		(50f, 0f)
	);
	// Configuration
	public static readonly RangeConfigEntry<float> LowHealthSoundVolume = new(ConfigSide.ClientOnly, 1f, (0f, 1f), "Awareness");
	public static readonly RangeConfigEntry<float> LowHealthFilteringIntensity = new(ConfigSide.ClientOnly, 1f, (0f, 1f), "Awareness");

	private SlotId lowHealthSoundSlot;
	private float lowHealthEffectIntensity;

	public override void Load()
	{
		AudioEffectsSystem.SetEnabledForSoundStyle(LowHealthSound, false);
	}

	public override void PostUpdate() => Update();

	public override void UpdateDead() => PostUpdate();

	private void Update()
	{
		if (!Player.IsLocal()) {
			return;
		}

		UpdateLowHealthEffects();
	}

	private void UpdateLowHealthEffects()
	{
		float goalLowHealthEffectIntensity = LowHealthEffectGradient.GetValue(Player.statLife);

		lowHealthEffectIntensity = MathUtils.StepTowards(lowHealthEffectIntensity, goalLowHealthEffectIntensity, 0.75f * TimeSystem.LogicDeltaTime);

		// Audio filtering
		float filteringIntensity = lowHealthEffectIntensity * LowHealthFilteringIntensity.Value;
		
		if (filteringIntensity > 0f) {
			AudioEffectsSystem.AddAudioEffectModifier(
				30,
				$"{nameof(TerrariaOverhaul)}/{nameof(PlayerHealthEffects)}",
				(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters musicParameters) => {
					float usedIntensity = filteringIntensity * intensity;

					soundParameters.LowPassFiltering += usedIntensity * 0.75f;
					musicParameters.LowPassFiltering += usedIntensity;
					musicParameters.Volume *= 1f - usedIntensity;
				}
			);
		}

		// Sound
		float soundVolume = Player.dead ? 0f : lowHealthEffectIntensity * LowHealthSoundVolume.Value;

		SoundUtils.UpdateLoopingSound(ref lowHealthSoundSlot, in LowHealthSound, soundVolume, CameraSystem.ScreenCenter);
	}
}
