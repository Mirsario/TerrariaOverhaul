// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;
using EnvironmentTag = TerrariaOverhaul.Core.Tags.Tag<TerrariaOverhaul.Common.Ambience.EnvironmentSystem>;

namespace TerrariaOverhaul.Common.Ambience;

[Autoload(Side = ModSide.Client)]
public sealed class AmbienceSystem : ModSystem
{
	public static readonly ConfigEntry<bool> EnableAmbientSounds = new(ConfigSide.ClientOnly, true, "Ambience");

	private static readonly EnvironmentTag VolumeTag = "Volume";
	private static readonly List<AmbienceTrackType> TrackTypes = new();
	private static readonly AmbienceTrackInstance[] TrackInstances = new AmbienceTrackInstance[64];
	private static BitMask<ulong> globalInstanceMask;

	public override void Load()
	{
		LoadAmbienceTracksFromMod(Mod);
	}

	public override void PostUpdateEverything()
	{
		var tracksSpan = CollectionsMarshal.AsSpan(TrackTypes);
		bool isAmbienceEnabled = EnableAmbientSounds;

		for (int i = 0; i < tracksSpan.Length; i++) {
			ref var type = ref tracksSpan[i];
			ref readonly var desc = ref type.Description;

			type.TargetVolume = CalculateTrackTargetVolume(in desc);
			type.CurrentVolume = MathUtils.StepTowards(type.CurrentVolume, type.TargetVolume, desc.VolumeChangeSpeed * TimeSystem.LogicDeltaTime);
			bool isActive = type.CurrentVolume > 0f;

			static uint RollCooldown(ExponentialRange? range)
				=> range is { } r ? (uint)(r.RandomValue() * TimeSystem.LogicFramerate) : 0;

			// Create new instances.
			if (isActive) {
				while (type.InstanceCount < desc.MaxInstances && globalInstanceMask.TrailingOneCount() is { } freeIndex && freeIndex < globalInstanceMask.Size) {
					TrackInstances[freeIndex] = new AmbienceTrackInstance {
						TypeIndex = (ushort)i,
						PlaybackCooldown = RollCooldown(desc.InstanceCooldown),
						Position = null,
					};
					type.InstanceMask.Set(freeIndex);
					globalInstanceMask.Set(freeIndex);
				}
			}

			// Update active instances.
			foreach (int index in type.InstanceMask) {
				ref var instance = ref TrackInstances[index];

				if (instance.PlaybackCooldown > 0)
					instance.PlaybackCooldown--;

				SoundEngine.TryGetActiveSound(instance.Slot, out var sound);

				if (isActive && isAmbienceEnabled) {
					if (sound == null) {
						if (instance.PlaybackCooldown == 0) {
							instance.Slot = SoundEngine.PlaySound(in desc.Sound, instance.Position);
							instance.PlaybackCooldown = RollCooldown(desc.InstanceCooldown);
						}

						continue;
					}

					sound.Position = instance.Position;
					sound.Volume = type.CurrentVolume;
				} else {
					sound?.Stop();
					instance.Slot = SlotId.Invalid;
					type.InstanceMask.Unset(index);
					globalInstanceMask.Unset(index);
				}
			}
		}
	}

	// This parses '.prefab.hjson' files in a mod and looks for 'EntityName: { AmbienceTrack: { ... } }' constructs in them.
	// Later, if the mod needs more data-driven approaches, it's possible to implement a universal ECS-like entity data storage not limited to ambience tracks.
	public static void LoadAmbienceTracksFromMod(Mod mod)
	{
		var assets = mod.GetFileNames();
		var jsonSerializer = new JsonSerializer();
		jsonSerializer.Converters.Add(new AmbienceTrackJsonConverter());

		const string Extension = ".prefab.hjson";

		foreach (string fullFilePath in assets.Where(t => t.EndsWith(Extension))) {
			using var stream = mod.GetFileStream(fullFilePath);
			using var streamReader = new StreamReader(stream);

			string fileName = Path.GetFileName(fullFilePath);
			string trackName = fileName.Substring(0, fileName.Length - Extension.Length);
			string hjsonText = streamReader.ReadToEnd();
			string jsonText = Hjson.HjsonValue.Parse(hjsonText).ToString();
			var json = JObject.Parse(jsonText)!;

			if (json["AmbienceTrack"] is JObject ambienceTrackJson) {
				using (new Logging.QuietExceptionHandle()) {
					try {
						RegisterAmbienceTrack(trackName, ambienceTrackJson.ToObject<AmbienceTrack>(jsonSerializer)!);
					}
					catch (Exception e) {
						DebugSystem.Log($"Failed to parse '{fullFilePath}':\r\n{e.Message}");
					}
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

	private static void RegisterAmbienceTrack(string name, AmbienceTrack desc)
	{
		VerifyAmbienceTrack(name, in desc);

		if (desc.DisableSoundFiltering) {
			AudioEffectsSystem.SetEnabledForSoundStyle(desc.Sound, false);
		}

		if (desc.SoundIsWallOccluded) {
			WallSoundOcclusion.SetEnabledForSoundStyle(desc.Sound, true);
		}

		TrackTypes.Add(new AmbienceTrackType {
			Name = name,
			Description = desc,
		});
	}

	private static void VerifyAmbienceTrack(string name, in AmbienceTrack track)
	{
		if (!track.Variables.Any(v => v.Output == VolumeTag)) {
			DebugSystem.Log($"Warning - Ambience track {name} does not declare a '{VolumeTag.Name}' variable!");
		}
	}
}
