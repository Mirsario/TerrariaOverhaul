using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.AudioEffects;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	[Autoload(Side = ModSide.Client)]
	public sealed class PlayerHealthEffects : ModPlayer
	{
		public static readonly SoundStyle LowHealthSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Player/LowHealthLoop", volume: 0.75f);
		public static readonly Gradient<float> LowHealthEffectGradient = new(
			(0f, 1f),
			(40f, 1f),
			(50f, 0f),
			(100f, 0f)
		);

		private SlotId lowHealthSoundSlot;
		private float lowHealthEffectIntensity;
		private float lowHealthBleedingCounter;

		public override void PostUpdate()
		{
			if(!Player.IsLocal()) {
				return;
			}

			UpdateLowHealthEffects();
		}

		private void UpdateLowHealthEffects()
		{
			float goalLowHealthEffectIntensity = LowHealthEffectGradient.GetValue(Player.statLife);

			lowHealthEffectIntensity = MathUtils.StepTowards(lowHealthEffectIntensity, goalLowHealthEffectIntensity, 0.75f * TimeSystem.LogicDeltaTime);

			if(lowHealthEffectIntensity > 0) {
				float addedLowPassFiltering = lowHealthEffectIntensity * 0.8f;

				AudioEffectsSystem.AddAudioEffectModifier(
					30,
					$"{nameof(TerrariaOverhaul)}/{nameof(PlayerHealthEffects)}",
					(float intensity, ref float _, ref float lowPassFilteringIntensity) => lowPassFilteringIntensity += addedLowPassFiltering * intensity
				);
			}

			//Sound
			SoundUtils.UpdateLoopingSound(ref lowHealthSoundSlot, LowHealthSound, lowHealthEffectIntensity, Player.Center);

			//Bleeding
			lowHealthBleedingCounter += lowHealthEffectIntensity / 4f;

			while(lowHealthBleedingCounter >= 1f) {
				var dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Blood);

				lowHealthBleedingCounter--;
			}
		}
	}
}
