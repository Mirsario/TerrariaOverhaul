using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Awareness
{
	[Autoload(Side = ModSide.Client)]
	public class PlayerManaEffects : ModPlayer
	{
		public static readonly SoundStyle ManaRegenSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/ManaRegenLoop") {
			Volume = 0.03f
		};
		public static readonly SoundStyle LowManaSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/LowManaLoop") {
			Volume = 0.33f
		};
		public static readonly Gradient<float> LowManaVolumeGradient = new(
			(0f, 1f),
			(0.1f, 1f),
			(0.333f, 0.5f),
			(0.375f, 0f),
			(1f, 0f)
		);
		// Configuration
		public static readonly RangeConfigEntry<float> LowManaSoundVolume = new(ConfigSide.ClientOnly, "Awareness", nameof(LowManaSoundVolume), 0f, 1f, () => 1f);
		public static readonly RangeConfigEntry<float> ManaRegenerationSoundVolume = new(ConfigSide.ClientOnly, "Awareness", nameof(ManaRegenerationSoundVolume), 0f, 1f, () => 1f);

		private SlotId lowManaSoundSlot;
		private SlotId manaRegenSoundSlot;
		private float lowManaEffectIntensity;
		private float lowManaDustCounter;
		private float manaRegenEffectIntensity;
		private float manaRegenDustCounter;

		public override void PreUpdate()
		{
			if (!Player.IsLocal()) {
				return;
			}

			UpdateLowManaEffects();
			UpdateManaRegenEffects();
		}

		private void UpdateLowManaEffects()
		{
			float goalLowManaEffectIntensity;

			if (!Player.dead) {
				float manaFactor = Player.statMana / (float)Player.statManaMax2;

				goalLowManaEffectIntensity = LowManaVolumeGradient.GetValue(manaFactor);
			} else {
				goalLowManaEffectIntensity = 0f;
			}

			lowManaEffectIntensity = MathUtils.StepTowards(lowManaEffectIntensity, goalLowManaEffectIntensity, 0.75f * TimeSystem.LogicDeltaTime);

			// Sound
			float lowManaSoundVolume = lowManaEffectIntensity * LowManaSoundVolume.Value;

			SoundUtils.UpdateLoopingSound(ref lowManaSoundSlot, LowManaSound, lowManaSoundVolume, CameraSystem.ScreenCenter);

			// Dust
			if (!Player.dead) {
				lowManaDustCounter += lowManaEffectIntensity / 4f;

				while (lowManaDustCounter >= 1f) {
					var dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.SomethingRed, Alpha: 255, Scale: Main.rand.NextFloat(1.5f, 2f));

					dust.noLight = true;
					dust.noGravity = true;
					dust.velocity *= 0.25f;

					lowManaDustCounter--;
				}
			} else {
				lowManaDustCounter = 0f;
			}
		}

		private void UpdateManaRegenEffects()
		{
			float goalManaRegenEffectIntensity;

			if (!Player.dead) {
				float manaFactor = Player.statMana / (float)Player.statManaMax2;
				float regenSpeed = Player.manaRegen + Player.manaRegenBonus;

				goalManaRegenEffectIntensity = manaFactor < 1f ? MathHelper.Clamp(regenSpeed / 30f, 0f, 1f) : 0f;
			} else {
				goalManaRegenEffectIntensity = 0f;
			}

			manaRegenEffectIntensity = MathUtils.StepTowards(manaRegenEffectIntensity, goalManaRegenEffectIntensity, 0.75f * TimeSystem.LogicDeltaTime);

			// Sound
			float manaRegenSoundVolume = manaRegenEffectIntensity * ManaRegenerationSoundVolume.Value;
			
			SoundUtils.UpdateLoopingSound(ref manaRegenSoundSlot, ManaRegenSound, manaRegenSoundVolume, CameraSystem.ScreenCenter);

			// Dust
			if (!Player.dead) {
				manaRegenDustCounter += manaRegenEffectIntensity / 4f;

				while (manaRegenDustCounter >= 1f) {
					var dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.ManaRegeneration, Alpha: 255, Scale: Main.rand.NextFloat(2f, 2.6f));

					dust.noLight = true;
					dust.noGravity = true;
					dust.velocity *= 0.5f;

					manaRegenDustCounter--;
				}
			} else {
				manaRegenDustCounter = 0f;
			}
		}
	}
}
