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
