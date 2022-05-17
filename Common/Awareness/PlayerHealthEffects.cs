using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Awareness
{
	[Autoload(Side = ModSide.Client)]
	public sealed class PlayerHealthEffects : ModPlayer
	{
		public static readonly ISoundStyle LowHealthSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/LowHealthLoop", volume: 1f);
		public static readonly Gradient<float> LowHealthEffectGradient = new(
			(0f, 1f),
			(30f, 1f),
			(50f, 0f)
		);
		// Configuration
		public static readonly RangeConfigEntry<float> LowHealthSoundVolume = new(ConfigSide.ClientOnly, "Awareness", nameof(LowHealthSoundVolume), 0f, 1f, () => 1f);
		public static readonly RangeConfigEntry<float> LowHealthFilteringIntensity = new(ConfigSide.ClientOnly, "Awareness", nameof(LowHealthFilteringIntensity), 0f, 1f, () => 1f);

		private SlotId lowHealthSoundSlot;
		private float lowHealthEffectIntensity;

		public override void Load()
		{
			AudioEffectsSystem.IgnoreSoundStyle(LowHealthSound);
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

			SoundUtils.UpdateLoopingSound(ref lowHealthSoundSlot, LowHealthSound, soundVolume, CameraSystem.ScreenCenter);
		}
	}
}
