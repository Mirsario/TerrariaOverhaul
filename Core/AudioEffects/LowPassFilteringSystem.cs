using System;
using System.Reflection;
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Core.AudioEffects;

[Autoload(Side = ModSide.Client)]
public sealed class LowPassFilteringSystem : ModSystem
{
	public static readonly ConfigEntry<bool> EnableLowPassFiltering = new(ConfigSide.ClientOnly, "Ambience", nameof(EnableLowPassFiltering), () => true) {
		RequiresRestart = true,
	};

	private static Action<SoundEffectInstance, float>? applyLowPassFilteringFunc;

	public static bool Enabled { get; private set; }

	public override void OnModLoad()
	{
		Enabled = false;

		if (!EnableLowPassFiltering) {
			DebugSystem.Log($"{GetType().Name} disabled: '{EnableLowPassFiltering.Category}.{EnableLowPassFiltering.Name}' is 'false'.");
			return;
		}

		if (!SoundEngine.IsAudioSupported) {
			DebugSystem.Log($"{GetType().Name} disabled: '{nameof(SoundEngine)}.{nameof(SoundEngine.IsAudioSupported)}' returned false.");
			return;
		}

		applyLowPassFilteringFunc = typeof(SoundEffectInstance)
			.GetMethod("INTERNAL_applyLowPassFilter", BindingFlags.Instance | BindingFlags.NonPublic)
			?.CreateDelegate<Action<SoundEffectInstance, float>>();

		if (applyLowPassFilteringFunc == null) {
			DebugSystem.Log($"{GetType().Name} disabled: Internal FNA methods are missing.");
			return;
		}

		Enabled = true;

		DebugSystem.Log($"{GetType().Name} enabled.");
	}

	internal static void ApplyEffects(SoundEffectInstance instance, in AudioEffectParameters parameters)
	{
		if (Enabled) {
			applyLowPassFilteringFunc!(instance, 1f - parameters.LowPassFiltering * 0.9f);
		}
	}
}
