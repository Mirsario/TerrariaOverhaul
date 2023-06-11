using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Music;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AudioEffects;

//TODO: Add configuration.
[Autoload(Side = ModSide.Client)]
public class AudioEffectsSystem : ModSystem
{
	public struct SoundData
	{
		public readonly WeakReference<SoundEffectInstance> Instance;
		public readonly WeakReference<ActiveSound>? TrackedSound;
		public readonly Vector2? StartPosition;
		public readonly SoundStyle SoundStyle;

		public AudioEffectParameters Parameters = new();

		public SoundData(SoundEffectInstance instance, Vector2? initialPosition = null, ActiveSound? trackedSound = null)
		{
			Instance = new WeakReference<SoundEffectInstance>(instance);
			TrackedSound = trackedSound != null ? new WeakReference<ActiveSound>(trackedSound) : null;
			StartPosition = initialPosition;
			SoundStyle = trackedSound?.Style ?? default;
		}
	}

	public delegate void SoundUpdateCallback(Span<SoundData> sounds); 

	public static readonly ConfigEntry<bool> EnableAudioFiltering = new(ConfigSide.ClientOnly, "Ambience", nameof(EnableAudioFiltering), () => true);

	private static readonly List<AudioEffectsModifier> modifiers = new();
	private static readonly List<SoundData> trackedSoundInstances = new();
	private static readonly HashSet<SoundStyle> soundStylesToIgnore = new() {
		SoundID.Grab,
		SoundID.MenuOpen,
		SoundID.MenuClose,
		SoundID.MenuTick,
		SoundID.Chat,
	};

	private static AudioEffectParameters soundParameters = new();
	private static AudioEffectParameters musicParameters = new();
	// Reflection
	private static Action<SoundEffectInstance, float>? applyReverbFunc;
	private static Action<SoundEffectInstance, float>? applyLowPassFilteringFunc;
	private static FieldInfo? soundEffectBasedAudioTrackInstanceField;
	private static string? audioErrorMessage;

	public static bool IsEnabled { get; private set; }
	public static bool ReverbEnabled { get; private set; }
	public static bool LowPassFilteringEnabled { get; private set; }

	public static event SoundUpdateCallback? OnSoundUpdate;

	public override void OnModLoad()
	{
		IsEnabled = false;
		ReverbEnabled = false;
		LowPassFilteringEnabled = false;

		WorldGen.Hooks.OnWorldLoad += TryAnnounceErrorMessage;

		if (!EnableAudioFiltering) {
			DebugSystem.Log($"Audio effects disabled: '{EnableAudioFiltering.Category}.{EnableAudioFiltering.Name}' is 'false'.");
			return;
		}

		if (!SoundEngine.IsAudioSupported) {
			DebugSystem.Log($"Audio effects disabled: '{nameof(SoundEngine)}.{nameof(SoundEngine.IsAudioSupported)}' returned false.");
			return;
		}

		applyReverbFunc = typeof(SoundEffectInstance)
			.GetMethod("INTERNAL_applyReverb", BindingFlags.Instance | BindingFlags.NonPublic)
			?.CreateDelegate<Action<SoundEffectInstance, float>>();

		applyLowPassFilteringFunc = typeof(SoundEffectInstance)
			.GetMethod("INTERNAL_applyLowPassFilter", BindingFlags.Instance | BindingFlags.NonPublic)
			?.CreateDelegate<Action<SoundEffectInstance, float>>();

		soundEffectBasedAudioTrackInstanceField = typeof(ASoundEffectBasedAudioTrack)
			.GetField("_soundEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic);

		if (applyReverbFunc == null || applyLowPassFilteringFunc == null || soundEffectBasedAudioTrackInstanceField == null) {
			DebugSystem.Log("Audio effects disabled: Internal FNA methods are missing.");
			return;
		}
		
		if (!TestAudioFiltering(out string? errorMessage)) {
			audioErrorMessage = errorMessage;

			DebugSystem.Log($"Audio effects disabled: '{errorMessage}'.");
			
			return;
		}

		// Injections
		IL_ActiveSound.Play += ActiveSoundPlayInjection;

		// Events
		MusicControlSystem.OnTrackUpdate += OnMusicTrackUpdate;

		// Mark as enabled
		IsEnabled = true;
		ReverbEnabled = true;
		LowPassFilteringEnabled = true;

		DebugSystem.Log("Audio effects enabled.");
	}

	public override void Unload()
	{
		OnSoundUpdate = null;
		WorldGen.Hooks.OnWorldLoad -= TryAnnounceErrorMessage;
	}

	public override void PostUpdateEverything()
	{
		// Update global values

		var newSoundParameters = new AudioEffectParameters();
		var newMusicParameters = new AudioEffectParameters();

		for (int i = 0; i < modifiers.Count; i++) {
			var modifier = modifiers[i];

			modifier.Modifier(modifier.TimeLeft / (float)modifier.TimeMax, ref newSoundParameters, ref newMusicParameters);

			if (--modifier.TimeLeft <= 0) {
				modifiers.RemoveAt(i--);
			} else {
				modifiers[i] = modifier;
			}
		}

		soundParameters = newSoundParameters;
		musicParameters = newMusicParameters;

		if (IsEnabled && OnSoundUpdate != null) {
			// Update sound instances
			lock (trackedSoundInstances) {
				UpdateSounds();
			}

			// Update music tracks
			UpdateMusic();
		}
	}

	public static void AddAudioEffectModifier(int time, string identifier, AudioEffectsModifier.ModifierDelegate func)
	{
		int existingIndex = modifiers.FindIndex(m => m.Id == identifier);

		if (existingIndex < 0) {
			modifiers.Add(new AudioEffectsModifier(time, identifier, func));
			return;
		}

		var modifier = modifiers[existingIndex];

		modifier.TimeLeft = Math.Max(modifier.TimeLeft, time);
		modifier.TimeMax = Math.Max(modifier.TimeMax, time);
		modifier.Modifier = func;

		modifiers[existingIndex] = modifier;
	}

	public static void IgnoreSoundStyle(SoundStyle ISoundStyle)
		=> soundStylesToIgnore.Add(ISoundStyle);

	private static void UpdateSounds()
	{
		var sounds = CollectionsMarshal.AsSpan(trackedSoundInstances);

		// Reset parameters
		for (int i = 0; i < sounds.Length; i++) {
			sounds[i].Parameters = soundParameters;
		}

		// Modify parameters
		OnSoundUpdate!(sounds);

		// Apply effects
		for (int i = 0; i < sounds.Length; i++) {
			ref var data = ref sounds[i];

			if (!data.Instance.TryGetTarget(out var soundInstance)) {
				trackedSoundInstances.RemoveAt(i--);

				// The above shouldn't recreate the collection, so the span is fine if shortened.
				sounds = sounds.Slice(0, sounds.Length - 1);

				continue;
			}

			ApplyEffects(soundInstance, in data.Parameters);
		}
	}

	private static void UpdateMusic()
	{
		if (Main.audioSystem is not LegacyAudioSystem legacyAudioSystem) {
			return;
		}

		for (int i = 0; i < legacyAudioSystem.AudioTracks.Length; i++) {
			if (legacyAudioSystem.AudioTracks[i] is not ASoundEffectBasedAudioTrack soundEffectTrack) {
				continue;
			}

			var instance = soundEffectBasedAudioTrackInstanceField!.GetValue(soundEffectTrack) as DynamicSoundEffectInstance;

			if (instance?.IsDisposed == false) {
				ApplyEffects(instance, in musicParameters);
			}
		}
	}

	private static void ApplyEffects(SoundEffectInstance instance, in AudioEffectParameters parameters)
	{
		if (ReverbEnabled) {
			applyReverbFunc!(instance, parameters.Reverb);
		}

		if (LowPassFilteringEnabled) {
			applyLowPassFilteringFunc!(instance, 1f - (parameters.LowPassFiltering * 0.9f));
		}
	}

	private static void OnMusicTrackUpdate(bool isActiveTrack, int trackIndex, ref float musicVolume, ref float musicFade)
	{
		musicVolume *= musicParameters.Volume;
	}

	private static bool TestAudioFiltering([NotNullWhen(false)] out string? errorMessage)
	{
		if (Main.audioSystem is not LegacyAudioSystem { Engine: AudioEngine engine }) {
			errorMessage = "Unable to get AudioEngine instance to test audio filtering.";

			return false;
		}

		IntPtr audioHandle = IntPtr.Zero;
		var audioHandleObj = typeof(AudioEngine)
			.GetField("handle", BindingFlags.Instance | BindingFlags.NonPublic)?
			.GetValue(engine);

		if (audioHandleObj != null) {
			audioHandle = (IntPtr)audioHandleObj;
		}

		if (audioHandle == IntPtr.Zero) {
			errorMessage = "Unable to get audio engine handle to test audio filtering.";

			return false;
		}

		_ = FAudio.FAudio_GetDeviceDetails(audioHandle, 0, out var deviceDetails);

		// Couldn't come up with anything better than this:
		if (deviceDetails.OutputFormat.Format.nSamplesPerSec > 48000) {
			errorMessage = "Audio device is set to a frequency higher than 48000Hz - assuming that FAudio would crash.";

			return false;
		}

		// These tests were useless, as the native exception cannot be caught in managed code.
		// There isn't even an AccessViolationException -- Native code just kills the process on failure.
		// See: https://github.com/tModLoader/FAudio/blob/master/src/FAudio.c?ts=4#L1442

		/*
		try {
			isTestingAudioFiltering = true;

			var testSound = ModContent.Request<SoundEffect>("Terraria/Sounds/Camera", AssetRequestMode.ImmediateLoad).Value;
			using var testSoundInstance = testSound.CreateInstance();

			testSoundInstance.Volume = 0f; // Quiet!

			// Apply
			ApplyEffects(testSoundInstance, new AudioEffectParameters { LowPassFiltering = 0.5f, Reverb = 0.5f });

			testSoundInstance.Play();

			// Undo
			ApplyEffects(testSoundInstance, new AudioEffectParameters { });

			testSoundInstance.Stop(true);
		}
		catch (Exception e) {
			errorMessage = $"{e.GetType().Name} - {e.Message}";
			
			return false;
		}
		finally {
			isTestingAudioFiltering = false;
		}
		*/

		errorMessage = null;

		return true;
	}

	private static void TryAnnounceErrorMessage()
	{
		if (audioErrorMessage != null) {
			Main.NewText(Language.GetTextValue($"Mods.{nameof(TerrariaOverhaul)}.Notifications.AudioFilteringTestFailure"), Color.MediumVioletRed);
			Main.NewText($@"""{audioErrorMessage}""", Color.PaleVioletRed);

			audioErrorMessage = null;
		}
	}

	private static void ActiveSoundPlayInjection(ILContext context)
	{
		var il = new ILCursor(context);

		int soundEffectInstanceLocalId = 0;

		il.GotoNext(
			MoveType.Before,
			i => i.MatchLdloc(out soundEffectInstanceLocalId),
			i => i.MatchCallvirt(typeof(SoundEffectInstance), nameof(SoundEffectInstance.Play))
		);

		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldloc, soundEffectInstanceLocalId);
		il.EmitDelegate<Action<ActiveSound, SoundEffectInstance>>(static (activeSound, soundEffectInstance) => {
			if (!IsEnabled) {
				return;
			}

			if (soundEffectInstance?.IsDisposed == false) {
				if (soundStylesToIgnore.Contains(activeSound.Style)) {
					return;
				}

				lock (trackedSoundInstances) {
					trackedSoundInstances.Add(new SoundData(soundEffectInstance, activeSound.Position, activeSound));
				}

				ApplyEffects(soundEffectInstance, in soundParameters);
			}
		});
	}
}
