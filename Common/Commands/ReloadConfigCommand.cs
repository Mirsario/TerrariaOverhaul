using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Commands
{
	public class ReloadConfigCommand : ModCommand
	{
		public override string Command => "oReloadConfig";
		public override string Description => "Reloads Overhaul's config from disk";
		public override CommandType Type => CommandType.Chat | CommandType.Console;

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (ConfigSystem.LoadConfig()) {
				MessageUtils.NewText($"Config successfully reloaded");
			} else {
				MessageUtils.NewText($"Config loading had errors.");
			}
		}
	}
}
