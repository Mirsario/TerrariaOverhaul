using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using ReLogic.Content;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public enum ConfigMediaKind : byte
{
	Image = 1,
	Video = 2,
	Any = Image | Video,
}

public static class ConfigMediaLookup
{
	public static bool TryGetMediaPath(IConfigEntry configEntry, out (string mediaPath, ConfigMediaKind kind) result, ConfigMediaKind filter = ConfigMediaKind.Any)
		=> TryGetMediaPath(configEntry.Category, configEntry.Name, out result, filter);

	public static bool TryGetMediaPath(string categoryName, string entryName, out (string mediaPath, ConfigMediaKind kind) result, ConfigMediaKind filter = ConfigMediaKind.Any)
	{
		string assetLocation = $"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Config";

		string thumbnailPath = $"{assetLocation}/{categoryName}/{entryName}";
		string thumbnailVideoPath = $"{thumbnailPath}Video";

		if (filter.HasFlag(ConfigMediaKind.Video) && ModContent.HasAsset(thumbnailVideoPath)) {
			result = (thumbnailVideoPath, ConfigMediaKind.Video);
			return true;
		}
		
		if (filter.HasFlag(ConfigMediaKind.Image) && ModContent.HasAsset(thumbnailPath)) {
			result = (thumbnailPath, ConfigMediaKind.Image);
			return true;
		}

		result = default;
		return false;
	}

	public static bool TryGetMedia(IConfigEntry configEntry, out (object mediaAsset, ConfigMediaKind kind) result, ConfigMediaKind filter = ConfigMediaKind.Any)
		=> TryGetMedia(configEntry.Category, configEntry.Name, out result, filter);

	public static bool TryGetMedia(string categoryName, string entryName, out (object mediaAsset, ConfigMediaKind kind) result, ConfigMediaKind filter = ConfigMediaKind.Any)
	{
		if (!TryGetMediaPath(categoryName, entryName, out var tuple, filter)) {
			result = default;
			return false;
		}

		result.kind = tuple.kind;
		result.mediaAsset = tuple.kind switch {
			ConfigMediaKind.Image => ModContent.Request<Texture2D>(tuple.mediaPath, AssetRequestMode.ImmediateLoad),
			ConfigMediaKind.Video => ModContent.Request<Video>(tuple.mediaPath, AssetRequestMode.ImmediateLoad),
			_ => throw new InvalidOperationException("Invalid enum"),
		};

		return true;
	}
}
