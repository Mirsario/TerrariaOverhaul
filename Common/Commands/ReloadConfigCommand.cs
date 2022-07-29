using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Commands;

public class ReloadConfigCommand : ModCommand
{
	public override string Command => "oReloadConfig";
	public override string Description => "Reloads Overhaul's config from disk";
	public override CommandType Type => CommandType.Chat | CommandType.Console;

	public override void Action(CommandCaller caller, string input, string[] args)
	{
		var (result, resultMessage) = ConfigSystem.LoadConfig(resetOnError: false);

		Color color;

		if (result.HasFlag(ConfigSystem.LoadingResult.ErrorFlag)) {
			color = Color.IndianRed;
		} else if (result.HasFlag(ConfigSystem.LoadingResult.WarningFlag)) {
			color = Color.OrangeRed;
		} else {
			color = Color.SpringGreen;
		}

		MessageUtils.NewText(resultMessage, color);
	}
}
