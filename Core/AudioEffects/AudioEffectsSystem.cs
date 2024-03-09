using System;
using System.Collections.Generic;
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
using TerrariaOverhaul.Common.Music;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Core.AudioEffects;

//TODO: Add configuration.
[Autoload(Side = ModSide.Client)]
public sealed class AudioEffectsSystem : ModSystem
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

	private static readonly List<AudioEffectsModifier> modifiers = new();
	private static readonly List<SoundData> trackedSoundInstances = new();
	private static readonly HashSet<SoundStyle> soundStylesToIgnore = new() {
		SoundID.Grab,
		SoundID.MenuOpen,
		SoundID.MenuClose,
		SoundID.MenuTick,
		SoundID.Chat,
	};

	private static readonly List<string> audioErrorMessages = new();
	private static AudioEffectParameters soundParameters = new();
	private static AudioEffectParameters musicParameters = new();
	private static FieldInfo? soundEffectBasedAudioTrackInstanceField;

	public static bool IsEnabled { get; private set; }

	public static event SoundUpdateCallback? OnSoundUpdate;

	public override void OnModLoad()
	{
		IsEnabled = false;

		WorldGen.Hooks.OnWorldLoad += TryAnnounceErrorMessage;

		if (!SoundEngine.IsAudioSupported) {
			DebugSystem.Log($"Audio effects disabled: '{nameof(SoundEngine)}.{nameof(SoundEngine.IsAudioSupported)}' returned false.");
			return;
		}

		soundEffectBasedAudioTrackInstanceField = typeof(ASoundEffectBasedAudioTrack)
			.GetField("_soundEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic);

		if (soundEffectBasedAudioTrackInstanceField == null) {
			DebugSystem.Log("Audio effects disabled: Internal FNA methods are missing.");
			return;
		}

		// Injections
		IL_ActiveSound.Play += ActiveSoundPlayInjection;
		// Events
		MusicControlSystem.OnTrackUpdate += OnMusicTrackUpdate;

		// Mark as enabled
		IsEnabled = true;

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

	public static void SetEnabledForSoundStyle(in SoundStyle soundStyle, bool value)
	{
		if (value) {
			soundStylesToIgnore.Remove(soundStyle);
		} else {
			soundStylesToIgnore.Add(soundStyle);
		}
	}

	internal static void AddAudioError(string errorMessage)
	{
		audioErrorMessages.Add(errorMessage);
	}

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
		ReverbSystem.ApplyEffects(instance, in parameters);
		LowPassFilteringSystem.ApplyEffects(instance, in parameters);
	}

	private static void OnMusicTrackUpdate(bool isActiveTrack, int trackIndex, ref float musicVolume, ref float musicFade)
	{
		musicVolume *= musicParameters.Volume;
	}

	private static void TryAnnounceErrorMessage()
	{
		if (audioErrorMessages.Count != 0) {
			Main.NewText(Language.GetTextValue($"Mods.{nameof(TerrariaOverhaul)}.Notifications.AudioFilteringTestFailure"), Color.MediumVioletRed);

			for (int i = 0; i < audioErrorMessages.Count; i++) {
				Main.NewText($@"""{audioErrorMessages[i]}""", Color.PaleVioletRed);
			}

			audioErrorMessages.Clear();
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
