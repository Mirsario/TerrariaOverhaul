using System;
using Newtonsoft.Json;
using ReLogic.Utilities;
using Terraria.Audio;

namespace TerrariaOverhaul.Common.Ambience;

public struct AmbienceTrack
{
	public string Name = string.Empty;
	public SlotId InstanceReference = SlotId.Invalid;
	public float VolumeChangeSpeed = 0.5f;
	public float Volume;
	public bool IsLooped = true;
	public bool SoundIsWallOccluded;
	public bool DisableSoundFiltering;

	[JsonRequired]
	public SoundStyle Sound;

	[JsonRequired]
	[JsonConverter(typeof(CalculatedSignalArrayJsonConverter))]
	public CalculatedSignal[] Variables = Array.Empty<CalculatedSignal>();

	public AmbienceTrack() { }
}
