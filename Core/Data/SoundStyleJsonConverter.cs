// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.Audio;

namespace TerrariaOverhaul.Core.Data;

internal sealed class SoundStyleJsonConverter : JsonConverter
{
	public override bool CanWrite => false;
	public override bool CanConvert(Type objectType) => objectType == typeof(SoundStyle) || objectType == typeof(SoundStyle?);
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType != JsonToken.StartObject) throw new InvalidOperationException($"Expected a JSON object, but got '{reader.TokenType}' instead.");

		var jObject = JObject.Load(reader);
		var nullableResult = jObject.ToObject<SoundStyle?>();
		if (!nullableResult.HasValue) {
			if (objectType != typeof(SoundStyle?)) {
				throw new NullReferenceException("Null sound style encountered.");
			}
			
			return nullableResult;
		}

		var result = nullableResult.Value;

		if (jObject["NumVariants"] is JValue numVariants)
			result.Variants = Enumerable.Range(1, Convert.ToInt32(numVariants.Value)).ToArray();

		return result;
	}
}
