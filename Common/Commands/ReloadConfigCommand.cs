// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Commands;

internal class ReloadConfigCommand : ModCommand
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
