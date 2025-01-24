// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using ReLogic.Content;

namespace TerrariaOverhaul.Utilities;

internal static class AssetExtensions
{
	public static Asset<T> EnsureLoaded<T>(this Asset<T> asset) where T : class
	{
		asset.Wait?.Invoke();

		return asset;
	}
}
