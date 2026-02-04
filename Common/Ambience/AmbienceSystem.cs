// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Api.Utilities;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Xna;
using EnvironmentTag = TerrariaOverhaul.Core.Tags.Tag<TerrariaOverhaul.Common.Ambience.EnvironmentSystem>;

namespace TerrariaOverhaul.Common.Ambience;

[Autoload(Side = ModSide.Client)]
internal sealed class AmbienceSystem : ModSystem
{
	public static readonly ConfigEntry<bool> EnableAmbientSounds = new(ConfigSide.ClientOnly, true, "Ambience");

	private static readonly EnvironmentTag VolumeTag = "Volume";
	private static readonly AmbienceTrackInstance[] TrackInstances = new AmbienceTrackInstance[64];
	private static readonly Dictionary<Prefab, DataEntity> tracksByPrefab = [];
	private static readonly Query trackQuery = Entities.Query().With<AmbienceTrackState>();
	private static readonly Query prefabQuery = Prefabs.Query().With<AmbienceTrack>();
	private static BitMask<ulong> globalInstanceMask;

	public AmbienceSystem()
	{
		Prefabs.RegisterJsonConverter(new AmbienceTrackJsonConverter());
	}

	public override void OnModLoad()
	{
		foreach (var prefab in prefabQuery) if (!tracksByPrefab.ContainsKey(prefab))
			CreateTrack(prefab);
	}

	private static DataEntity CreateTrack(Prefab prefab)
	{
		ref readonly var dsc = ref prefab.Get<AmbienceTrack>();
		string trackName = prefab.Has<PrefabInfo>() ? prefab.Get<PrefabInfo>() : "Unknown";

		VerifyTrack(trackName, in dsc);

		AudioEffectsSystem.SetEnabledForSoundStyle(dsc.Sound, !dsc.DisableSoundFiltering);
		WallSoundOcclusion.SetEnabledForSoundStyle(dsc.Sound, dsc.SoundIsWallOccluded);

		var track = Entities.Create();
		track.Add(new AmbienceTrackState { Prefab = prefab });
		tracksByPrefab[prefab] = track;
		return track;
	}
	private static void RemoveTrack(DataEntity track)
	{
		tracksByPrefab.Remove(track.Get<AmbienceTrackState>().Prefab);
		track.Destroy();
	}
	private static void VerifyTrack(string name, in AmbienceTrack track)
	{
		if (!track.Variables.Any(v => v.Output == VolumeTag)) {
			DebugSystem.Logger.Warn($"Ambience track {name} does not declare a '{VolumeTag.Name}' variable!");
		}
	}

	public override void PostUpdateEverything()
	{
		bool isAmbienceEnabled = EnableAmbientSounds;

#if DEBUG
		// Add tracks from new prefabs.
		foreach (var prefab in prefabQuery) if (!tracksByPrefab.ContainsKey(prefab))
			CreateTrack(prefab);
#endif

		foreach (var track in trackQuery) {
			ref var state = ref track.Get<AmbienceTrackState>();

#if DEBUG
			// Remove tracks with deleted or malformed prefabs.
			if (!state.Prefab.IsValid || !state.Prefab.Has<AmbienceTrack>()) {
				RemoveTrack(track);
				continue;
			}
#endif
			
			ref readonly var desc = ref state.Description;

			state.TargetVolume = CalculateTrackTargetVolume(in desc);
			state.CurrentVolume = MathUtils.StepTowards(state.CurrentVolume, state.TargetVolume, desc.VolumeChangeSpeed * TimeSystem.LogicDeltaTime);
			bool isActive = state.CurrentVolume > 0f;

			static uint RollCooldown(ExponentialRange? range)
				=> range is { } r ? (uint)(r.Translate(Main.rand.NextFloat()) * TimeSystem.LogicFramerate) : 0;

			// Shutdown sounds in case the prefab is updated or stops existing.
			uint? generation = state.Prefab.Has<PrefabInfo>() ? state.Prefab.Get<PrefabInfo>().Generation : null;
			bool UpdateCallback(ActiveSound sound)
				=> track.IsValid && (generation == null || generation == track.Get<AmbienceTrackState>().Prefab.Get<PrefabInfo>().Generation);

			// Create new instances.
			if (isActive) {
				while (state.InstanceCount < desc.MaxInstances && globalInstanceMask.TrailingOneCount() is { } freeIndex && freeIndex < globalInstanceMask.Size) {
					TrackInstances[freeIndex] = new AmbienceTrackInstance {
						Type = track,
						PlaybackCooldown = RollCooldown(desc.InstanceCooldown),
						Position = null,
					};
					state.InstanceMask.Set(freeIndex);
					globalInstanceMask.Set(freeIndex);
				}
			}

			// Update active instances.
			foreach (int index in state.InstanceMask) {
				ref var instance = ref TrackInstances[index];

				if (instance.PlaybackCooldown > 0)
					instance.PlaybackCooldown--;

				SoundEngine.TryGetActiveSound(instance.Slot, out var sound);

				if (isActive && isAmbienceEnabled) {
					if (sound == null) {
						if (instance.PlaybackCooldown == 0) {
							var style = desc.Sound with {
								PauseBehavior = PauseBehavior.PauseWithGame,
							};
							
							instance.Slot = SoundEngine.PlaySound(in style, instance.Position, UpdateCallback);
							instance.PlaybackCooldown = RollCooldown(desc.InstanceCooldown);
						}

						continue;
					}

					sound.Position = instance.Position;
					sound.Volume = state.CurrentVolume;
				} else {
					sound?.Stop();
					instance.Slot = SlotId.Invalid;
					state.InstanceMask.Unset(index);
					globalInstanceMask.Unset(index);
				}
			}
		}
	}

	public static float CalculateTrackTargetVolume(in AmbienceTrack track)
	{
		float volume = 0f;
		var variables = track.Variables;

		for (int i = 0; i < variables.Length; i++) {
			ref var variable = ref variables[i];
			ref float value = ref variable.Value;
			var operation = variable.Operation;
			var modifiers = variable.Modifiers;
			var inputs = variable.Inputs;

			value = 0f;

			for (int j = 0; j < inputs.Length; j++) {
				var inputTag = inputs[j];

				if (!EnvironmentSystem.TryGetSignal(inputTag, out float input)) {
					// Navigate back in the first loop to find the correct value
					for (int ii = i - 1; ii >= 0; ii--) {
						if (variables[ii].Output == inputTag) {
							input = variables[ii].Value;
							break;
						}
					}
				}

				if (j == 0) {
					value = input;
					continue;
				}

				value = operation switch {
					SignalOperation.Multiply => value * input,
					SignalOperation.Addition => value * input,
					SignalOperation.Max => Math.Max(value, input),
					SignalOperation.Min => Math.Min(value, input),
					_ => throw new NotImplementedException(),
				};
			}

			if (modifiers.HasFlag(SignalModifiers.Inverse)) {
				value = 1f - value;
			}

			if (variable.Output == VolumeTag) {
				volume = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		return volume;
	}
}
