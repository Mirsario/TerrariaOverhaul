using System;
using log4net;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Debugging
{
	public sealed class DebugSystem : ModSystem
	{
		private static ILog logger;

		public static ILog Logger => logger ?? (logger = LogManager.GetLogger(nameof(TerrariaOverhaul)));

		public static void Log(object text, bool toChat = false, bool toConsole = false, bool toFile = true)
		{
			string actualText = text?.ToString();

			if(toChat) {
				Main.NewText(actualText);
			}

			if(toFile) {
				Logger.Info(actualText);
			}

			if(toConsole || Main.dedServ && toFile) {
				Console.WriteLine(actualText);

				if(Main.dedServ) {
					return;
				}
			}
		}
	}
}
