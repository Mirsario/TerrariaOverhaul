using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntitySources;
using TerrariaOverhaul.Common.Systems.AudioEffects;
using TerrariaOverhaul.Common.Systems.Camera;
using TerrariaOverhaul.Common.Systems.Gores;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
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

		private SlotId lowHealthSoundSlot;
		private float lowHealthEffectIntensity;
		private float lowHealthBleedingCounter;

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

			//Audio filtering
			if (lowHealthEffectIntensity > 0) {
				float effectIntensityCopy = lowHealthEffectIntensity;

				AudioEffectsSystem.AddAudioEffectModifier(
					30,
					$"{nameof(TerrariaOverhaul)}/{nameof(PlayerHealthEffects)}",
					(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters musicParameters) => {
						float usedIntensity = effectIntensityCopy * intensity;

						soundParameters.LowPassFiltering += usedIntensity * 0.75f;
						musicParameters.LowPassFiltering += usedIntensity;
						musicParameters.Volume *= 1f - usedIntensity;
					}
				);
			}

			//Sound
			float soundVolume = Player.dead ? 0f : lowHealthEffectIntensity;

			SoundUtils.UpdateLoopingSound(ref lowHealthSoundSlot, LowHealthSound, soundVolume, CameraSystem.ScreenCenter);

			//Bleeding
			if (!Player.dead) {
				lowHealthBleedingCounter += lowHealthEffectIntensity / 4f;
				IEntitySource entitySource = new EntitySource_EntityBleeding(Player);

				while (lowHealthBleedingCounter >= 1f) {
					var dust = Dust.NewDustDirect(entitySource, Player.position, Player.width, Player.height, DustID.Blood);

					lowHealthBleedingCounter--;
				}
			}
		}
	}
}
