using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Hjson;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience;

[Autoload(Side = ModSide.Client)]
public sealed class AmbienceSystem : ModSystem
{
	public static readonly ConfigEntry<bool> EnableAmbientSounds = new(ConfigSide.ClientOnly, "Ambience", nameof(EnableAmbientSounds), () => true);

	private static readonly List<AmbienceTrack> Tracks = new();
	private static readonly Tag VolumeTag = "Volume";

	public override void Load()
	{
		LoadAmbienceTracksFromMod(Mod);
	}

	public override void PostUpdateEverything()
	{
		var tracksSpan = CollectionsMarshal.AsSpan(Tracks);
		bool isAmbienceEnabled = EnableAmbientSounds;

		for (int i = 0; i < tracksSpan.Length; i++) {
			ref var track = ref tracksSpan[i];

			track.Volume = MathUtils.StepTowards(track.Volume, CalculateTrackTargetVolume(in track), track.VolumeChangeSpeed * TimeSystem.LogicDeltaTime);

			SoundEngine.TryGetActiveSound(track.InstanceReference, out var soundInstance);

			if (!isAmbienceEnabled || track.Volume <= 0.0f) {
				if (soundInstance != null) {
					soundInstance.Stop();

					track.InstanceReference = SlotId.Invalid;
				}

				continue;
			}

			SoundUtils.UpdateLoopingSound(ref track.InstanceReference, in track.Sound, track.Volume, null);
		}
	}

	// This parses .prefab files in a mod and looks for 'EntityName: { AmbienceTrack: { ... } }' constructs in them.
	// Later, if the mod needs more data-driven approaches, it's possible to implement a universal ECS-like entity data storage not limited to ambience tracks.
	public static void LoadAmbienceTracksFromMod(Mod mod)
	{
		var assets = mod.GetFileNames();

		foreach (string fullFilePath in assets.Where(t => t.EndsWith(".prefab"))) {
			using var stream = mod.GetFileStream(fullFilePath);
			using var streamReader = new StreamReader(stream);

			string hjsonText = streamReader.ReadToEnd();
			string jsonText = HjsonValue.Parse(hjsonText).ToString(Stringify.Plain);
			var json = JToken.Parse(jsonText);

			foreach (var rootToken in json) {
				if (rootToken is not JProperty { Name: string entityName, Value: JObject entityJson }
				|| entityJson["AmbienceTrack"] is not JObject ambienceTrackJson) {
					continue;
				}

				using (new Logging.QuietExceptionHandle()) {
					try {
						RegisterAmbienceTrack(entityName, ambienceTrackJson.ToObject<AmbienceTrack>());
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
				float input;

				if (!EnvironmentSystem.TryGetSignal(inputTag, out input)) {
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

	private static void RegisterAmbienceTrack(string name, AmbienceTrack track)
	{
		track.Name = name;

		VerifyAmbienceTrack(in track);

		if (track.SoundIsWallOccluded) {
			WallSoundOcclusion.SetEnabledForSoundStyle(track.Sound, true);
		}

		Tracks.Add(track);
	}

	private static void VerifyAmbienceTrack(in AmbienceTrack track)
	{
		if (!track.Variables.Any(v => v.Output == VolumeTag)) {
			DebugSystem.Log($"Warning - Ambience track {track.Name} does not declare a '{VolumeTag.Name}' variable!");
		}
	}
}
