// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TerrariaOverhaul.Api.Utilities;

[JsonConverter(typeof(NewtonsoftJsonConverter))]
public struct ExponentialRange()
{
	/// <summary> Minimum range. </summary>
	public float Min;
	/// <summary> Maximum range. </summary>
	public float Max;
	/// <summary> The power that the distance factor will be raised to. </summary>
	public float Exponent = 1f;

	public ExponentialRange(float Min, float Max, float Exponent = 1f) : this()
	{
		this.Min = Min;
		this.Max = Max;
		this.Exponent = Exponent;
	}

	/// <summary> Maps a [0..1] factor to the [Min..Max] range, with the exponent applied. </summary>
	public readonly float Translate(float value01)
		=> Min + (Max - Min) * (1f - MathF.Pow(1f - value01, Exponent));

	/// <summary> Given a distance value, calculates the [0..1] distance factor using this range's parameters. </summary>
	public readonly float DistanceFactor(float distance)
	{
		if (distance < Min) return 1f;

		float factor = 1f - MathF.Min(1f, MathF.Max(0f, distance - Min) / (Max - Min));
		if (!float.IsNormal(factor)) return 0f;

		return MathF.Pow(factor, Exponent);
	}

	private sealed class NewtonsoftJsonConverter : JsonConverter
	{
		public override bool CanWrite => false;
		public override bool CanConvert(Type objectType) => objectType == typeof(ExponentialRange);
		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.StartObject) throw new InvalidOperationException($"Expected a JSON object, but got '{reader.TokenType}' instead.");

			var jObject = JObject.Load(reader);
			var result = new ExponentialRange();

			if (jObject[nameof(Min)] is JValue min) result.Min = Convert.ToSingle(min.Value);
			if (jObject[nameof(Max)] is JValue max) result.Max = Convert.ToSingle(max.Value);
			if (jObject[nameof(Exponent)] is JValue exp) result.Exponent = Convert.ToSingle(exp.Value);

			return result;
		}
	}
}
