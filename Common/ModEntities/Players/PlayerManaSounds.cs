using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	[Autoload(Side = ModSide.Client)]
	public class PlayerManaSounds : ModPlayer
	{
		public static readonly SoundStyle LowManaSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Magic/LowManaLoop", volume: 0.5f);
		public static readonly Gradient<float> LowManaVolumeGradient = new Gradient<float>(
			(0f, 1f),
			(0.1f, 1f),
			(0.333f, 0.5f),
			(0.375f, 0f),
			(1f, 0f)
		);

		private SlotId lowManaSoundSlot;
		private float lowManaSoundVolume;

		public override void PreUpdate()
		{
			float manaFactor = Player.statMana / (float)Player.statManaMax2;
			float goalLowManaVolume = LowManaVolumeGradient.GetValue(manaFactor);

			lowManaSoundVolume = MathUtils.StepTowards(lowManaSoundVolume, goalLowManaVolume, 3f * TimeSystem.LogicDeltaTime);

			var sound = lowManaSoundSlot.IsValid ? SoundEngine.GetActiveSound(lowManaSoundSlot) : null;

			if(lowManaSoundVolume > 0f) {
				if(sound == null) {
					lowManaSoundSlot = SoundEngine.PlayTrackedSound(LowManaSound, Player.Center);
					sound = SoundEngine.GetActiveSound(lowManaSoundSlot);
				}

				sound.Position = Player.Center;
				sound.Volume = lowManaSoundVolume * 0.5f;
			} else if(sound != null) {
				sound.Stop();

				lowManaSoundSlot = SlotId.Invalid;
			}
		}
	}
}
