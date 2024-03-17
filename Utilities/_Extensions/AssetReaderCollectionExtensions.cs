using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Readers;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Utilities;

internal static class AssetReaderCollectionExtensions
{
	private static readonly FieldInfo? readersByExtensionField = typeof(AssetReaderCollection)
		.GetField("_readersByExtension", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

	private static readonly FieldInfo? extensionsField = typeof(AssetReaderCollection)
		.GetField("_extensions", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

	public static void RemoveExtension(this AssetReaderCollection collection, string extension)
	{
		if (readersByExtensionField?.GetValue(collection) is not Dictionary<string, IAssetReader> dictionary
		|| extensionsField?.GetValue(collection) is not string[]) {
			DebugSystem.Logger.Warn("Unable to remove a reader from the asset reader collection.");
			return;
		}

		// And then we hope that nothing explodes.
		dictionary.Remove(extension);
		extensionsField.SetValue(collection, dictionary.Keys.ToArray());
	}
}
