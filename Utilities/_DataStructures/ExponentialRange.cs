// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.Utilities;

namespace TerrariaOverhaul.Utilities;

[JsonConverter(typeof(NewtonsoftJsonConverter))]
public struct ExponentialRange()
{
	public float Min;
	public float Max;
	public float Exponent = 1f;

	public ExponentialRange(float Min, float Max, float Exponent = 1f) : this()
	{
		this.Min = Min;
		this.Max = Max;
		this.Exponent = Exponent;
	}

	public readonly float RandomValue() => RandomValue(Main.rand);
	public readonly float RandomValue(UnifiedRandom rng) => Translate(rng.NextFloat());

	public readonly float Translate(float value01)
	{
		return MathHelper.Lerp(Min, Max, 1f - MathF.Pow(1f - value01, Exponent));
	}

	public readonly float DistanceFactor(float distance)
	{
		if (distance < Min) return 1f;

		float factor = 1f - MathF.Min(1f, MathF.Max(0f, distance - Min) / (Max - Min));
		if (!float.IsNormal(factor)) return 0f;

		return MathF.Pow(factor, Exponent);
	}

	public sealed class NewtonsoftJsonConverter : JsonConverter
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
