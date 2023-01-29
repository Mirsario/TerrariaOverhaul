using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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
//TODO: Split occlusion and other effect logic into other classes.
[Autoload(Side = ModSide.Client)]
public class AudioEffectsSystem : ModSystem
{
	private struct SoundInstanceData
	{
		public readonly WeakReference<SoundEffectInstance> Instance;
		public readonly WeakReference<ActiveSound>? TrackedSound;
		public readonly Vector2? StartPosition;

		public bool FirstUpdate;
		public float LocalLowPassFiltering;
		public float TargetLocalLowPassFiltering;

		public SoundInstanceData(SoundEffectInstance instance, Vector2? initialPosition = null, ActiveSound? trackedSound = null)
		{
			Instance = new WeakReference<SoundEffectInstance>(instance);
			TrackedSound = trackedSound != null ? new WeakReference<ActiveSound>(trackedSound) : null;
			StartPosition = initialPosition;

			FirstUpdate = true;
			TargetLocalLowPassFiltering = LocalLowPassFiltering = 0f;
		}
	}

	private const int FullAudioUpdateThreshold = 4;

	public static readonly ConfigEntry<bool> EnableAudioFiltering = new(ConfigSide.ClientOnly, "Ambience", nameof(EnableAudioFiltering), () => true);

	private static readonly List<AudioEffectsModifier> Modifiers = new();
	private static readonly List<SoundInstanceData> TrackedSoundInstances = new();
	private static readonly HashSet<SoundStyle> SoundStylesToIgnore = new() {
		SoundID.Grab,
		SoundID.MenuOpen,
		SoundID.MenuClose,
		SoundID.MenuTick,
		SoundID.Chat,
	};
	private static readonly HashSet<SoundStyle> SoundStylesWithWallOcclusion = new() {
		SoundID.Bird,
	};

	private static float playerWallOcclusionCache;
	private static AudioEffectParameters soundParameters = new();
	private static AudioEffectParameters musicParameters = new();
	// Reflection
	private static Action<SoundEffectInstance, float>? applyReverbFunc;
	private static Action<SoundEffectInstance, float>? applyLowPassFilteringFunc;
	private static FieldInfo? soundEffectBasedAudioTrackInstanceField;
	private static string? audioErrorMessage;
	
#pragma warning disable CS0169
#pragma warning disable IDE0044
	private static bool isTestingAudioFiltering;
#pragma warning restore

	public static bool IsEnabled { get; private set; }
	public static bool ReverbEnabled { get; private set; }
	public static bool LowPassFilteringEnabled { get; private set; }

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

		IsEnabled = true;
		ReverbEnabled = true;
		LowPassFilteringEnabled = true;

		// Track 'active' sounds, and apply effects before they get played.
		IL.Terraria.Audio.ActiveSound.Play += context => {
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
					if (SoundStylesToIgnore.Contains(activeSound.Style)) {
						return;
					}

					TrackedSoundInstances.Add(new SoundInstanceData(soundEffectInstance, activeSound.Position, activeSound));

					ApplyEffects(soundEffectInstance, soundParameters);
				}
			});
		};

		MusicControlSystem.OnTrackUpdate += OnMusicTrackUpdate;

		DebugSystem.Log("Audio effects enabled.");
	}

	public override void Unload()
	{
		WorldGen.Hooks.OnWorldLoad -= TryAnnounceErrorMessage;
	}

	public override void PostUpdateEverything()
	{
		// Update global values

		var newSoundParameters = new AudioEffectParameters();
		var newMusicParameters = new AudioEffectParameters();

		for (int i = 0; i < Modifiers.Count; i++) {
			var modifier = Modifiers[i];

			modifier.Modifier(modifier.TimeLeft / (float)modifier.TimeMax, ref newSoundParameters, ref newMusicParameters);

			if (--modifier.TimeLeft <= 0) {
				Modifiers.RemoveAt(i--);
			} else {
				Modifiers[i] = modifier;
			}
		}

		soundParameters = newSoundParameters;
		musicParameters = newMusicParameters;

		playerWallOcclusionCache = Main.LocalPlayer.GetModPlayer<PlayerWallOcclusion>().OcclusionFactor;

		if (IsEnabled) {
			bool fullUpdate = Main.GameUpdateCount % FullAudioUpdateThreshold == 0;

			// Update sound instances
			for (int i = 0; i < TrackedSoundInstances.Count; i++) {
				var data = TrackedSoundInstances[i];

				if (!UpdateSoundData(ref data, fullUpdate)) {
					TrackedSoundInstances.RemoveAt(i--);
					continue;
				}

				TrackedSoundInstances[i] = data;
			}

			if (Main.audioSystem is LegacyAudioSystem legacyAudioSystem) {
				for (int i = 0; i < legacyAudioSystem.AudioTracks.Length; i++) {
					if (legacyAudioSystem.AudioTracks[i] is ASoundEffectBasedAudioTrack soundEffectTrack) {
						var instance = soundEffectBasedAudioTrackInstanceField!.GetValue(soundEffectTrack) as DynamicSoundEffectInstance;

						if (instance?.IsDisposed == false) {
							ApplyEffects(instance, musicParameters);
						}
					}
				}
			}
		}
	}

	public static void AddAudioEffectModifier(int time, string identifier, AudioEffectsModifier.ModifierDelegate func)
	{
		int existingIndex = Modifiers.FindIndex(m => m.Id == identifier);

		if (existingIndex < 0) {
			Modifiers.Add(new AudioEffectsModifier(time, identifier, func));
			return;
		}

		var modifier = Modifiers[existingIndex];

		modifier.TimeLeft = Math.Max(modifier.TimeLeft, time);
		modifier.TimeMax = Math.Max(modifier.TimeMax, time);
		modifier.Modifier = func;

		Modifiers[existingIndex] = modifier;
	}

	public static void IgnoreSoundStyle(SoundStyle ISoundStyle)
		=> SoundStylesToIgnore.Add(ISoundStyle);
	
	public static void EnableSoundStyleWallOcclusion(SoundStyle ISoundStyle)
		=> SoundStylesWithWallOcclusion.Add(ISoundStyle);

	private static void ApplyEffects(SoundEffectInstance instance, AudioEffectParameters parameters)
	{
		if (ReverbEnabled) {
			applyReverbFunc!(instance, parameters.Reverb);
		}

		if (LowPassFilteringEnabled) {
			applyLowPassFilteringFunc!(instance, 1f - (parameters.LowPassFiltering * 0.9f));
		}
	}

	private static bool UpdateSoundData(ref SoundInstanceData data, bool fullUpdate)
	{
		if (!data.Instance.TryGetTarget(out var instance) || instance.IsDisposed || instance.State != SoundState.Playing) {
			return false;
		}

		if (fullUpdate || data.FirstUpdate) {
			UpdateSoundOcclusion(ref data);
		}

		if (data.FirstUpdate) {
			data.LocalLowPassFiltering = data.TargetLocalLowPassFiltering;
		} else {
			data.LocalLowPassFiltering = MathHelper.Lerp(data.LocalLowPassFiltering, data.TargetLocalLowPassFiltering, 3f * TimeSystem.LogicDeltaTime);
		}

		var localParameters = soundParameters;

		localParameters.LowPassFiltering += data.LocalLowPassFiltering;

		ApplyEffects(instance, localParameters);

		data.FirstUpdate = false;

		return true;
	}

	private static void UpdateSoundOcclusion(ref SoundInstanceData data)
	{
		Vector2? soundPosition;
		ActiveSound? trackedSound = null;

		if (data.TrackedSound != null && data.TrackedSound.TryGetTarget(out trackedSound)) {
			soundPosition = trackedSound.Position;
		} else {
			soundPosition = data.StartPosition;
		}

		float occlusion = 0f;

		if (soundPosition.HasValue) {
			occlusion = MathHelper.Clamp(occlusion + CalculateSoundOcclusion(soundPosition.Value.ToTileCoordinates()), 0f, 1f);
		}

		if (trackedSound != null && SoundStylesWithWallOcclusion.Contains(trackedSound.Style)) {
			occlusion = MathHelper.Clamp(occlusion + playerWallOcclusionCache, 0f, 1f);
		}

		data.TargetLocalLowPassFiltering = occlusion;
	}

	private static float CalculateSoundOcclusion(Vector2Int position)
	{
		int occludingTiles = 0;

		const int MaxOccludingTiles = 15;

		GeometryUtils.BresenhamLine(
			CameraSystem.ScreenCenter.ToTileCoordinates(),
			position,
			(Vector2Int point, ref bool stop) => {
				if (!Main.tile.TryGet(point, out var tile)) {
					stop = true;
					return;
				}

				if (tile.HasTile && Main.tileSolid[tile.TileType]) {
					occludingTiles++;

					if (occludingTiles >= MaxOccludingTiles) {
						stop = true;
					}
				}
			}
		);

		return occludingTiles / (float)MaxOccludingTiles;
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
}
