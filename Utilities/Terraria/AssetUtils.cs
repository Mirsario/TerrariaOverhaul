// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReLogic.Content;
using ReLogic.Content.Readers;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Utilities.Terraria;

internal static class AssetUtils
{
	public static Asset<T> EnsureLoaded<T>(this Asset<T> asset) where T : class
	{
		asset.Wait?.Invoke();

		return asset;
	}

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
