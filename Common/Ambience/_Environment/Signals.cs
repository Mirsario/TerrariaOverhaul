// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EnvironmentTag = TerrariaOverhaul.Core.Tags.Tag<TerrariaOverhaul.Common.Ambience.EnvironmentSystem>;

namespace TerrariaOverhaul.Common.Ambience;

public enum SignalOperation
{
	Multiply,
	Addition,
	Max,
	Min,
}

[Flags]
public enum SignalModifiers
{
	None = 0,
	Inverse = 1,
}

internal struct CalculatedSignal
{
	[JsonRequired] public SignalOperation Operation;
	[JsonRequired] public EnvironmentTag[] Inputs;

	public EnvironmentTag Output;
	public SignalModifiers Modifiers;
	public float Value;
}

internal sealed class CalculatedSignalArrayJsonConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override bool CanConvert(Type objectType)
		=> objectType == typeof(CalculatedSignal[]);

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		=> throw new NotImplementedException();

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType != JsonToken.StartObject) {
			throw new InvalidOperationException($"Expected a JSON object, but got '{reader.TokenType}' instead.");
		}

		var jObject = JObject.Load(reader);
		var properties = jObject.Properties();
		var result = new CalculatedSignal[properties.Count()];
		int i = 0;

		foreach (var property in properties) {
			result[i++] = property.Value.ToObject<CalculatedSignal>(serializer) with {
				Output = (EnvironmentTag)property.Name
			};
		}

		return result;
	}
}
