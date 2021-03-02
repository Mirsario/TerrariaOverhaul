using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Configuration;

namespace TerrariaOverhaul.Common.Commands
{
	public class ReloadConfigCommand : ModCommand
	{
		public override string Command => "oReloadConfig";
		public override string Description => "Reloads Overhaul's config from disk";
		public override CommandType Type => CommandType.Chat | CommandType.Console;

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			var log = Main.dedServ
				? (Action<object>)Console.WriteLine
				: (obj => Main.NewText(obj));

			if(ConfigSystem.LoadConfig()) {
				log($"Config successfully reloaded");
			} else {
				log($"Config loading had errors.");
			}
		}
	}
}
