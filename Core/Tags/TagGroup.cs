using System;
using System.Collections.Generic;

namespace TerrariaOverhaul.Core.Tags;

// Horrible. Taken from an old 1.4_contenttags draft of TML. And made worse.

/// <summary> Derivatives of TagGroups are used to store and get tags associated with content IDs. </summary>
//[Autoload(true)]
public abstract class TagGroup // : ModType
{
	public abstract int TypeCount { get; }

	internal Dictionary<string, TagData> TagNameToData = new(StringComparer.InvariantCultureIgnoreCase);

	/// <summary>
	/// Returns a TagData instance, which can be used to modify and check for tags. <para/>
	/// <b>Be sure to cache the return value whenever possible!</b>
	/// </summary>
	/// <param name="tagName"> The name of the tag. </param>
	public TagData GetTag(string tagName)
	{
		if (!TagNameToData.TryGetValue(tagName, out var data)) {
			TagNameToData[tagName] = data = new TagData(TypeCount);
		}

		return data;
	}
}
