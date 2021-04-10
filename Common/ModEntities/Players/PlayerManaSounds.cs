using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	[Autoload(Side = ModSide.Client)]
	public class PlayerManaSounds : ModPlayer
	{
		public static readonly SoundStyle ManaRegenSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Magic/ManaRegenLoop", volume: 0.03f);
		public static readonly SoundStyle LowManaSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Magic/LowManaLoop", volume: 0.33f);
		public static readonly Gradient<float> LowManaVolumeGradient = new Gradient<float>(
			(0f, 1f),
			(0.1f, 1f),
			(0.333f, 0.5f),
			(0.375f, 0f),
			(1f, 0f)
		);

		private SlotId lowManaSoundSlot;
		private SlotId manaRegenSoundSlot;
		private float lowManaEffectIntensity;
		private float lowManaDustCounter;
		private float manaRegenEffectIntensity;
		private float manaRegenDustCounter;

		public override void PreUpdate()
		{
			UpdateLowManaEffects();
			UpdateManaRegenEffects();
		}

		private void UpdateLowManaEffects()
		{
			float manaFactor = Player.statMana / (float)Player.statManaMax2;
			float goalLowManaEffectIntensity = LowManaVolumeGradient.GetValue(manaFactor);

			lowManaEffectIntensity = MathUtils.StepTowards(lowManaEffectIntensity, goalLowManaEffectIntensity, 0.75f * TimeSystem.LogicDeltaTime);

			//Sound
			UpdateSound(ref lowManaSoundSlot, lowManaEffectIntensity, LowManaSound);

			//Dust
			lowManaDustCounter += lowManaEffectIntensity / 4f;

			while(lowManaDustCounter >= 1f) {
				var dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.SomethingRed, Alpha: 255, Scale: Main.rand.NextFloat(1.5f, 2f));

				dust.noLight = true;
				dust.noGravity = true;
				dust.velocity *= 0.25f;

				lowManaDustCounter--;
			}
		}
		private void UpdateManaRegenEffects()
		{
			float manaFactor = Player.statMana / (float)Player.statManaMax2;
			float regenSpeed = Player.manaRegen + Player.manaRegenBonus;
			float goalManaRegenEffectIntensity = manaFactor < 1f ? MathHelper.Clamp(regenSpeed / 30f, 0f, 1f) : 0f;

			manaRegenEffectIntensity = MathUtils.StepTowards(manaRegenEffectIntensity, goalManaRegenEffectIntensity, 0.75f * TimeSystem.LogicDeltaTime);

			//Sound
			UpdateSound(ref manaRegenSoundSlot, manaRegenEffectIntensity, ManaRegenSound);

			//Dust
			manaRegenDustCounter += manaRegenEffectIntensity / 4f;

			while(manaRegenDustCounter >= 1f) {
				var dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, 45, Alpha: 255, Scale: Main.rand.NextFloat(2f, 2.6f));

				dust.noLight = true;
				dust.noGravity = true;
				dust.velocity *= 0.5f;

				manaRegenDustCounter--;
			}
		}
		private void UpdateSound(ref SlotId slot, float volume, SoundStyle style)
		{
			var sound = slot.IsValid ? SoundEngine.GetActiveSound(slot) : null;

			if(volume > 0f) {
				float styleVolume = style.Volume;

				try {
					if(sound == null) {
						style.Volume = 0f;
						slot = SoundEngine.PlayTrackedSound(style, Player.Center);
						sound = SoundEngine.GetActiveSound(slot);

						if(sound == null) {
							return;
						}
					}

					sound.Position = Player.Center;
					sound.Volume = volume;
				}
				finally {
					style.Volume = styleVolume;
				}
			} else if(sound != null) {
				sound.Stop();

				slot = SlotId.Invalid;
			}
		}
	}
}
