using System;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Core.AudioEffects;

[Autoload(Side = ModSide.Client)]
public sealed class ReverbSystem : ModSystem
{
	public static readonly ConfigEntry<bool> EnableReverb = new(ConfigSide.ClientOnly, "Ambience", nameof(EnableReverb), () => true) {
		RequiresRestart = true,
	};

	private static Action<SoundEffectInstance, float>? applyReverbFunc;

	public static bool Enabled { get; private set; }

	public override void OnModLoad()
	{
		Enabled = false;

		if (!EnableReverb) {
			DebugSystem.Log($"{GetType().Name} disabled: '{EnableReverb.Category}.{EnableReverb.Name}' is 'false'.");
			return;
		}

		if (!SoundEngine.IsAudioSupported) {
			DebugSystem.Log($"{GetType().Name} disabled: '{nameof(SoundEngine)}.{nameof(SoundEngine.IsAudioSupported)}' returned false.");
			return;
		}

		applyReverbFunc = typeof(SoundEffectInstance)
			.GetMethod("INTERNAL_applyReverb", BindingFlags.Instance | BindingFlags.NonPublic)
			?.CreateDelegate<Action<SoundEffectInstance, float>>();

		if (applyReverbFunc == null) {
			DebugSystem.Log($"{GetType().Name} disabled: Internal FNA methods are missing.");
			return;
		}

		if (TestAudioFiltering() is string errorMessage) {
			AudioEffectsSystem.AddAudioError(errorMessage);
			DebugSystem.Log($"{GetType().Name} disabled: '{errorMessage}'.");
			return;
		}

		Enabled = true;

		DebugSystem.Log($"{GetType().Name} enabled.");
	}

	internal static void ApplyEffects(SoundEffectInstance instance, in AudioEffectParameters parameters)
	{
		//TODO: Check if channel count is too high??
		if (Enabled) {
			applyReverbFunc!(instance, parameters.Reverb);
		}
	}

	private static string? TestAudioFiltering()
	{
		if (Main.audioSystem is not LegacyAudioSystem { Engine: AudioEngine engine }) {
			return "Unable to get AudioEngine instance to test audio filtering.";
		}

		IntPtr audioHandle;
		object? audioHandleObj = typeof(AudioEngine)
			.GetField("handle", BindingFlags.Instance | BindingFlags.NonPublic)?
			.GetValue(engine);

		if (audioHandleObj == null || (audioHandle = (IntPtr)audioHandleObj) == IntPtr.Zero) {
			return "Unable to get audio engine handle to test audio filtering.";
		}

		_ = FAudio.FAudio_GetDeviceDetails(audioHandle, 0, out var deviceDetails);

		//var inputFormat = deviceDetails.OutputFormat.Format;
		var deviceFormat = deviceDetails.OutputFormat.Format;
		const int MinSampleRate = 20000;
		const int MaxSampleRate = 48000;

		if (deviceFormat.nSamplesPerSec < MinSampleRate || deviceFormat.nSamplesPerSec > MaxSampleRate) {
			return $"Audio device frequency is outside the [{MinSampleRate}..{MaxSampleRate}] range - assuming that FAudio will crash.";
		}

		if (!(
			deviceFormat.nChannels == 2
		//	(inputFormat.nChannels == 1 && (deviceFormat.nChannels == 1 || deviceFormat.nChannels == 6)) ||
		//	(inputFormat.nChannels == 2 && (deviceFormat.nChannels == 2 || deviceFormat.nChannels == 6)) ||
		//	(inputFormat.nChannels == 6 && deviceFormat.nChannels == 6)
		)) {
			return $"Unsupported audio device channel count ({deviceFormat.nChannels}) - assuming that FAudio will crash.";
		}

		return null;
	}
}
