using System;
using System.IO;
using log4net;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Debugging
{
	public sealed partial class DebugSystem : ModSystem
	{
		private static ILog logger;

		public static ILog Logger => logger ??= LogManager.GetLogger(nameof(TerrariaOverhaul));

		public static void Log(object text, bool toChat = false, bool toConsole = false, bool toFile = true)
		{
			string actualText = text?.ToString();

			if (toChat) {
				Main.NewText(actualText);
			}

			if (toFile) {
				Logger.Info(actualText);
			}

			if (toConsole || Main.dedServ && toFile) {
				Console.WriteLine(actualText);

				if (Main.dedServ) {
					return;
				}
			}
		}

		internal static void EnableMonoModDumps()
		{
			string dumpDir = Path.GetFullPath("MonoModDump");

			Directory.CreateDirectory(dumpDir);

			Environment.SetEnvironmentVariable("MONOMOD_DMD_DEBUG", "1");
			Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", dumpDir);
		}

		internal static void DisableMonoModDumps()
		{
			Environment.SetEnvironmentVariable("MONOMOD_DMD_DEBUG", "0");
			Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", null);
		}
	}
}
