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
		var result = ConfigIO.LoadConfig();

		Color color;
		string message;

		if (result.HasFlag(ConfigIO.Result.ErrorFlag)) {
			color = Color.IndianRed;
			message = $"Failed to load configuration: '{result}'.";
		} else if (result.HasFlag(ConfigIO.Result.WarningFlag)) {
			color = Color.OrangeRed;
			message = $"Configuration loaded with warnings: '{result}'.";
		} else {
			color = Color.SpringGreen;
			message = "Configuration loaded successfully";
		}

		MessageUtils.NewText(message, color);
	}
}
