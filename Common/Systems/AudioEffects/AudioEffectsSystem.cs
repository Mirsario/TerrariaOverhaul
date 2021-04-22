using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Configuration;
using TerrariaOverhaul.Core.Systems.Debugging;

namespace TerrariaOverhaul.Common.Systems.AudioEffects
{
	[Autoload(Side = ModSide.Client)]
	public class AudioEffectsSystem : ModSystem
	{
		private static readonly List<AudioEffectsModifier> Modifiers = new();

		private static float reverbIntensity;
		private static float lowPassFilteringIntensity;
		private static MethodInfo applyReverbMethod;
		private static MethodInfo applyLowPassMethod;
		private static object[] argArray;

		public static bool IsEnabled { get; private set; }
		public static bool ReverbEnabled { get; private set; }
		public static bool LowPassFilteringEnabled { get; private set; }

		public override void Load()
		{
			IsEnabled = false;

			applyReverbMethod = typeof(SoundEffectInstance).GetMethod("INTERNAL_applyReverb", BindingFlags.Instance | BindingFlags.NonPublic);
			applyLowPassMethod = typeof(SoundEffectInstance).GetMethod("INTERNAL_applyLowPassFilter", BindingFlags.Instance | BindingFlags.NonPublic);
			argArray = new object[1];
			IsEnabled = applyReverbMethod != null && applyLowPassMethod != null;

			On.Terraria.Audio.ActiveSound.Update += (orig, activeSound) => {
				orig(activeSound);

				if(activeSound.Sound?.IsDisposed == false) {
					UpdateSound(activeSound.Sound);
				}
			};

			DebugSystem.Log(IsEnabled ? "Audio effects enabled." : "Audio effects disabled: Internal FNA methods are missing.");
		}
		public override void PostUpdateEverything()
		{
			ReverbEnabled = true;
			LowPassFilteringEnabled = true;

			float newReverbIntensity = 0f;
			float newLowPassIntensity = 0f;

			for(int i = 0; i < Modifiers.Count; i++) {
				var modifier = Modifiers[i];

				modifier.Modifier(modifier.TimeLeft / (float)modifier.TimeMax, ref newReverbIntensity, ref newLowPassIntensity);

				if(--modifier.TimeLeft <= 0) {
					Modifiers.RemoveAt(i--);
				} else {
					Modifiers[i] = modifier;
				}
			}

			//TODO: Add configuration.
			reverbIntensity = MathHelper.Clamp(newReverbIntensity, 0f, 1f);
			lowPassFilteringIntensity = MathHelper.Clamp(newLowPassIntensity, 0f, 1f);

			Console.WriteLine($"Reverb: {reverbIntensity:0.00} | Low-pass: {lowPassFilteringIntensity:0.00}");

			UpdateAllSounds();
		}

		public static void UpdateSound(SoundEffectInstance soundInstance)
		{
			if(!IsEnabled || soundInstance == null) {
				return;
			}

			if(ReverbEnabled) {
				argArray[0] = reverbIntensity;
				applyReverbMethod.Invoke(soundInstance, argArray);
			}

			if(LowPassFilteringEnabled) {
				argArray[0] = 1f - lowPassFilteringIntensity;
				applyLowPassMethod.Invoke(soundInstance, argArray);
			}
		}
		public static void UpdateSoundArray(SoundEffectInstance[] soundInstances)
		{
			if(!IsEnabled || soundInstances == null) {
				return;
			}

			try {
				for(int i = 0; i < soundInstances.Length; i++) {
					var soundInstance = soundInstances[i];

					if(soundInstance != null && !soundInstance.IsDisposed && soundInstance.State == SoundState.Playing) {
						UpdateSound(soundInstance);
					}
				}
			}
			catch { }
		}
		public static void AddAudioEffectModifier(int time, string identifier, AudioEffectsModifier.ModifierDelegate func)
		{
			int existingIndex = Modifiers.FindIndex(m => m.Id == identifier);

			if(existingIndex < 0) {
				Modifiers.Add(new AudioEffectsModifier(time, identifier, func));
				return;
			}

			var modifier = Modifiers[existingIndex];

			modifier.TimeLeft = Math.Max(modifier.TimeLeft, time);
			modifier.TimeMax = Math.Max(modifier.TimeMax, time);
			modifier.Modifier = func;

			Modifiers[existingIndex] = modifier;
		}

		private static void UpdateAllSounds()
		{
			if(!IsEnabled) {
				return;
			}

			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceShatter);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceCamera);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceDoorClosed);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceDoorOpen);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceMaxMana);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceUnlock);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceRun);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceDoubleJump);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceCoins);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceDrown);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceMoonlordCry);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstancePixie);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstanceGrass);
			UpdateSound(SoundEngine.LegacySoundPlayer.SoundInstancePlayerKilled);

			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceZombie);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceRoar);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceSplash);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceLiquid);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceNpcKilled);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceTink);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceDig);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceMech);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceCoin);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceDrip);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceNpcHit);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceItem);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstancePlayerHit);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.SoundInstanceFemaleHit);
			UpdateSoundArray(SoundEngine.LegacySoundPlayer.TrackableSoundInstances);
		}
	}
}
