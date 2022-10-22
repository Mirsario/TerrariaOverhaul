using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Tags;

public sealed class TagSystem : ModSystem
{
	private static readonly Dictionary<string, int> tagIdsByString = new(StringComparer.InvariantCultureIgnoreCase);

	public override void Unload()
	{
		tagIdsByString.Clear();
	}

	public static int GetTagIdForString(string name)
	{
		if (!tagIdsByString.TryGetValue(name, out int id)) {
			tagIdsByString[name] = id = tagIdsByString.Count + 1;
		}

		return id;
	}
}
