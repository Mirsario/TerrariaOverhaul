using System.IO;
using System.Linq;
using System.Text;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ConfigurationScreen;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Commands;

#if DEBUG
internal sealed class DumpConfigIconsCommand : ModCommand
{
	public override string Command => "oDumpConfigIcons";
	public override string Description => "DEBUG - Dump a markdown file containing status of all config icons.";
	public override CommandType Type => CommandType.Chat | CommandType.Console;

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		const string UrlPrefix = "https://raw.githubusercontent.com/justreq/TerrariaOverhaul/1.4";
		const string AssetLocation = "Assets/Textures/UI/Config";

		var sb = new StringBuilder();

		foreach (var categoryPair in ConfigSystem.CategoriesByName.OrderBy(s => s.Key)) {
			sb.AppendLine($"### `{categoryPair.Key}`");

			foreach (var entry in categoryPair.Value.EntriesByName.OrderBy(s => s.Key).Select(s => s.Value)) {
				bool hasIcon = ConfigMediaLookup.TryGetMedia(entry, out var mediaTuple, ConfigMediaKind.Image);
				bool isVideo = hasIcon && mediaTuple.kind == ConfigMediaKind.Video;
				char symbol = hasIcon ? 'x' : ' ';
				string imgPath = hasIcon ? $"{entry.Category}/{entry.Name}.{(isVideo ? "ogv" : "png")}" : "UnknownOption.png";
				string imgEmbed = $"![]({UrlPrefix}/{AssetLocation}/{imgPath})";

				sb.AppendLine($"- [{symbol}] `{entry.Name}`");
				sb.AppendLine($"\t> {imgEmbed}");
			}
		}
		sb.AppendLine();

		File.WriteAllText("TerrariaOverhaul.ConfigIconDump.md", sb.ToString());
	}
}
#endif
