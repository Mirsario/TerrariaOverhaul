using System;
using System.Collections.Generic;
using System.Numerics;

namespace TerrariaOverhaul.Core.Tags;

internal struct StringIdMap
{
	private int nextId;
	private string[] stringLookup;
	private readonly Dictionary<string, int> idLookup;

	public readonly int NextId => nextId;
	public readonly ReadOnlySpan<string> StringLookup => stringLookup;
	public readonly Dictionary<string, int> IdLookup => idLookup;

	public StringIdMap() : this(capacity: 32) { }

	public StringIdMap(int capacity)
	{
		nextId = 1;
		idLookup = new(capacity, StringComparer.InvariantCultureIgnoreCase);
		stringLookup = new string[capacity];
	}

	public void Clear()
	{
		idLookup.Clear();
		Array.Clear(stringLookup);
		nextId = 1;
	}

	public readonly string StringFromId(int id)
	{
		if (id == 0 || id >= NextId) {
			throw new ArgumentOutOfRangeException(nameof(id));
		}

		return StringLookup[id] ?? throw new KeyNotFoundException(nameof(id));
	}

	public int IdFromString(string? str)
	{
		if (str == null || str.Length == 0) {
			return default;
		}

		if (!idLookup.TryGetValue(str, out int id)) {
			id = nextId;
			nextId += 1;

			Array.Resize(ref stringLookup, (int)BitOperations.RoundUpToPowerOf2((uint)(nextId + 1)));

			idLookup[str] = id;
			stringLookup[id] = str;
		}

		return id;
	}
}
