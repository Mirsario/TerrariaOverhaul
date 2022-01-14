using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Players;
using TerrariaOverhaul.Common.Systems.Camera;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.Systems.Debugging;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Systems.AudioEffects
{
	//TODO: Add configuration.
	[Autoload(Side = ModSide.Client)]
	public class AudioEffectsSystem : ModSystem
	{
		private struct SoundInstanceData
		{
			public readonly WeakReference<SoundEffectInstance> Instance;
			public readonly WeakReference<ActiveSound> TrackedSound;
			public readonly Vector2? StartPosition;

			public bool firstUpdate;
			public float localLowPassFiltering;
			public float targetLocalLowPassFiltering;

			public SoundInstanceData(SoundEffectInstance instance, Vector2? initialPosition = null, ActiveSound trackedSound = null)
			{
				Instance = new WeakReference<SoundEffectInstance>(instance);
				TrackedSound = trackedSound != null ? new WeakReference<ActiveSound>(trackedSound) : null;
				StartPosition = initialPosition;

				firstUpdate = true;
				targetLocalLowPassFiltering = localLowPassFiltering = 0f;
			}
		}

		private const int FullAudioUpdateThreshold = 4;

		private static readonly List<AudioEffectsModifier> Modifiers = new();
		private static readonly List<SoundInstanceData> TrackedSoundInstances = new();
		private static readonly HashSet<ISoundStyle> SoundStylesToIgnore = new() {
			new LegacySoundStyle(SoundID.Grab, -1),
			new LegacySoundStyle(SoundID.MenuOpen, -1),
			new LegacySoundStyle(SoundID.MenuClose, -1),
			new LegacySoundStyle(SoundID.MenuTick, -1),
			new LegacySoundStyle(SoundID.Chat, -1),
		};
		private static readonly HashSet<ISoundStyle> SoundStylesWithWallOcclusion = new() {
			new LegacySoundStyle(SoundID.Bird, -1),
		};

		private static float playerWallOcclusionCache;
		private static AudioEffectParameters soundParameters = AudioEffectParameters.Default;
		private static AudioEffectParameters musicParameters = AudioEffectParameters.Default;
		// Reflection
		private static Action<SoundEffectInstance, float> applyReverbFunc;
		private static Action<SoundEffectInstance, float> applyLowPassFilteringFunc;
		private static FieldInfo soundEffectBasedAudioTrackInstanceField;

		public static bool IsEnabled { get; private set; }
		public static bool ReverbEnabled { get; private set; }
		public static bool LowPassFilteringEnabled { get; private set; }

		public override void Load()
		{
			IsEnabled = false;

			applyReverbFunc = typeof(SoundEffectInstance)
				.GetMethod("INTERNAL_applyReverb", BindingFlags.Instance | BindingFlags.NonPublic)
				?.CreateDelegate<Action<SoundEffectInstance, float>>();

			applyLowPassFilteringFunc = typeof(SoundEffectInstance)
				.GetMethod("INTERNAL_applyLowPassFilter", BindingFlags.Instance | BindingFlags.NonPublic)
				?.CreateDelegate<Action<SoundEffectInstance, float>>();

			soundEffectBasedAudioTrackInstanceField = typeof(ASoundEffectBasedAudioTrack)
				.GetField("_soundEffectInstance", BindingFlags.Instance | BindingFlags.NonPublic);

			IsEnabled = applyReverbFunc != null && applyLowPassFilteringFunc != null;

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
				il.EmitDelegate<Action<ActiveSound, SoundEffectInstance>>((activeSound, soundEffectInstance) => {
					if (soundEffectInstance?.IsDisposed == false) {
						if (SoundStylesToIgnore.Contains(activeSound.Style)) {
							return;
						}

						TrackedSoundInstances.Add(new SoundInstanceData(soundEffectInstance, activeSound.Position, activeSound));

						ApplyEffects(soundEffectInstance, soundParameters);
					}
				});
			};

			// Track legacy sounds
			On.Terraria.Audio.LegacySoundPlayer.PlaySound += (orig, soundPlayer, type, x, y, style, volumeScale, pitchOffset) => {
				var result = orig(soundPlayer, type, x, y, style, volumeScale, pitchOffset);

				if (result != null && TrackedSoundInstances != null) {
					if (SoundStylesToIgnore.Any(s => s is LegacySoundStyle ls && ls.SoundId == type && (ls.Style == style || ls.Style <= 0))) {
						return result;
					}

					Vector2? position = x >= 0 && y >= 0 ? new Vector2(x, y) : null;

					TrackedSoundInstances.Add(new SoundInstanceData(result, position));
				}

				return result;
			};

			// Update volume of music.
			IL.Terraria.Main.UpdateAudio += context => {
				var il = new ILCursor(context);

				int volumeLocalId = 0;
				int iLocalId = 0;

				// Match 'float num2 = musicFade[i] * musicVolume * num;'
				il.GotoNext(
					MoveType.After,
					i => i.MatchLdsfld(typeof(Main), nameof(Main.musicFade)),
					i => i.MatchLdloc(out iLocalId),
					i => i.Match(OpCodes.Ldelem_R4),
					i => i.MatchLdsfld(typeof(Main), nameof(Main.musicVolume)),
					i => i.Match(OpCodes.Mul),
					i => i.MatchLdloc(out _),
					i => i.Match(OpCodes.Mul),
					i => i.MatchStloc(out volumeLocalId)
				);

				// Go into the start of the switch case. *Into* is to avoid dealing with jumps.
				il.GotoNext(
					MoveType.After,
					i => i.MatchLdloc(iLocalId)
				);

				// Emit code that pops 'i', modifies the local, and loads 'i' again.
				il.Emit(OpCodes.Pop);
				il.Emit(OpCodes.Ldloc, volumeLocalId);
				il.EmitDelegate<Func<float, float>>(volume => volume * musicParameters.Volume);
				il.Emit(OpCodes.Stloc, volumeLocalId);
				il.Emit(OpCodes.Ldloc, iLocalId);
			};

			DebugSystem.Log(IsEnabled ? "Audio effects enabled." : "Audio effects disabled: Internal FNA methods are missing.");
		}

		public override void PostUpdateEverything()
		{
			// Update global values
			ReverbEnabled = true;
			LowPassFilteringEnabled = true;

			AudioEffectParameters newSoundParameters = AudioEffectParameters.Default;
			AudioEffectParameters newMusicParameters = AudioEffectParameters.Default;

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
							var instance = (DynamicSoundEffectInstance)soundEffectBasedAudioTrackInstanceField.GetValue(soundEffectTrack);

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

		public static void IgnoreSoundStyle(ISoundStyle ISoundStyle) => SoundStylesToIgnore.Add(ISoundStyle);
		public static void EnableSoundStyleWallOcclusion(ISoundStyle ISoundStyle) => SoundStylesWithWallOcclusion.Add(ISoundStyle);

		private static void ApplyEffects(SoundEffectInstance instance, AudioEffectParameters parameters)
		{
			if (ReverbEnabled) {
				applyReverbFunc(instance, parameters.Reverb);
			}

			if (LowPassFilteringEnabled) {
				applyLowPassFilteringFunc(instance, 1f - (parameters.LowPassFiltering * 0.9f));
			}
		}

		private static bool UpdateSoundData(ref SoundInstanceData data, bool fullUpdate)
		{
			if (!data.Instance.TryGetTarget(out var instance) || instance.IsDisposed || instance.State != SoundState.Playing) {
				return false;
			}

			if (fullUpdate || data.firstUpdate) {
				UpdateSoundOcclusion(ref data);
			}

			if (data.firstUpdate) {
				data.localLowPassFiltering = data.targetLocalLowPassFiltering;
			} else {
				data.localLowPassFiltering = MathHelper.Lerp(data.localLowPassFiltering, data.targetLocalLowPassFiltering, 3f * TimeSystem.LogicDeltaTime);
			}

			var localParameters = soundParameters;

			localParameters.LowPassFiltering += data.localLowPassFiltering;

			ApplyEffects(instance, localParameters);

			data.firstUpdate = false;

			return true;
		}

		private static void UpdateSoundOcclusion(ref SoundInstanceData data)
		{
			Vector2? soundPosition;
			ActiveSound trackedSound = null;

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

			data.targetLocalLowPassFiltering = occlusion;
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

					if (tile.IsActive && Main.tileSolid[tile.type]) {
						occludingTiles++;

						if (occludingTiles >= MaxOccludingTiles) {
							stop = true;
						}
					}
				}
			);

			return occludingTiles / (float)MaxOccludingTiles;
		}
	}
}
