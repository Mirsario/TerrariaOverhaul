// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReLogic.Reflection;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience;

// This could be an immutable struct, but Newtonsoft.Json doesn't handle their field initializers correctly.
// In 2024 that is!
//[JsonConverter(typeof(AmbienceTrackJsonConverter))] // This isn't attached directly, as we need to be able to call the default converter.
public sealed class AmbienceTrack : IComponent
{
	public struct PositionInfo()
	{
		//	public ushort[] NPCs = [];
		public ushort[] Tiles = [];
		public byte[] Liquids = [];
	}

	public bool IsLooped = true;
	public bool SoundIsWallOccluded;
	public bool DisableSoundFiltering;
	public float VolumeChangeSpeed = 0.5f;
	public int MaxInstances = 1;
	public ExponentialRange InstanceCooldown = new();
	public PositionInfo? Positional;
	[JsonRequired] public SoundStyle Sound;
	[JsonRequired, JsonConverter(typeof(CalculatedSignalArrayJsonConverter))] public CalculatedSignal[] Variables = Array.Empty<CalculatedSignal>();

	public AmbienceTrack() { }
}

public struct AmbienceTrackType()
{
	public required string Name;
	public required AmbienceTrack Description;
	public float CurrentVolume;
	public float TargetVolume;
	public BitMask<ulong> InstanceMask;

	public readonly int InstanceCount => InstanceMask.PopCount();
}

public struct AmbienceTrackInstance()
{
	public required ushort TypeIndex;
	public required Vector2? Position;
	public SlotId Slot = SlotId.Invalid;
	public uint? PlaybackCooldown;
}

public sealed class AmbienceTrackJsonConverter : JsonConverter
{
	private static readonly IdDictionary liquidSearch = IdDictionary.Create(typeof(short), typeof(LiquidID));

	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
		=> objectType == typeof(AmbienceTrack);

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		=> throw new NotImplementedException();

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType != JsonToken.StartObject) throw new InvalidOperationException($"Expected a JSON object, but got '{reader.TokenType}' instead.");

		var jObject = JObject.Load(reader);
		var result = jObject.ToObject<AmbienceTrack>()!;

		if (jObject.Property("Positional") is { Value: JObject positional }) {
			var pos = new AmbienceTrack.PositionInfo();
			if (jObject.Property("Tiles") is { Value: JArray tiles })
				pos.Tiles = tiles.Select(j => GetTileId((string)j!)).ToArray();
			if (jObject.Property("Liquids") is { Value: JArray liquids })
				pos.Liquids = liquids.Select(j => (byte)liquidSearch.GetId((string)j!)).ToArray();

			result.Positional = pos;
		}

		return result;
	}

	private static ushort GetTileId(string identifier)
	{
		if (TileID.Search.TryGetId(identifier, out int id)) return (ushort)id;
		if (ModContent.TryFind(identifier, out ModTile tile)) return tile.Type;
		throw new InvalidOperationException($"Unknown tile: '{identifier}'.");
	}
}
