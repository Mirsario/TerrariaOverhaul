﻿using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	[Autoload(Side = ModSide.Client)]
	public class PlayerWaterEffects : ModPlayer
	{
		public static readonly ISoundStyle UnderwaterLoopSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Ambience/UnderwaterLoop", volume: 0.75f);

		private SlotId underwaterLoopSoundSlot;
		private float underwaterEffectIntensity;

		public override void PostUpdate()
		{
			if (!Player.IsLocal()) {
				return;
			}

			float goalUnderwaterEffectIntensity = Player.IsUnderwater() ? 1f : 0f;

			underwaterEffectIntensity = MathUtils.StepTowards(underwaterEffectIntensity, goalUnderwaterEffectIntensity, 0.75f * TimeSystem.LogicDeltaTime);

			// Audio filtering
			if (underwaterEffectIntensity > 0) {
				float addedLowPassFiltering = underwaterEffectIntensity;

				AudioEffectsSystem.AddAudioEffectModifier(
					60,
					$"{nameof(TerrariaOverhaul)}/{nameof(PlayerWaterEffects)}",
					(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters _) => {
						soundParameters.LowPassFiltering += addedLowPassFiltering * intensity;
					}
				);
			}

			// Sound
			SoundUtils.UpdateLoopingSound(ref underwaterLoopSoundSlot, UnderwaterLoopSound, underwaterEffectIntensity, CameraSystem.ScreenCenter);
		}
	}
}
