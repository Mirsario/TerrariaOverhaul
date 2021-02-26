using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Exceptions;
using TerrariaOverhaul.Core.Systems.Networking;

namespace TerrariaOverhaul
{
	public partial class OverhaulMod : Mod
	{
		public static readonly uint BetaNumber = 1;
		public static readonly bool IsBeta = BetaNumber > 0;
		public static readonly string VersionSuffix = $"(ALPHA {BetaNumber})";
		public static readonly string PersonalDirectory = Path.Combine(Main.SavePath, "TerrariaOverhaul");
		public static readonly Version MinimalTMLVersion = new Version("0.12");
		public static readonly Assembly Assembly;
		public static readonly Assembly EngineAssembly;
		public static readonly Assembly TMLAssembly;
		public static readonly Type[] AssemblyTypes;

		//internal static readonly ResourceManager ResourceManager = new ResourceManager("TerrariaOverhaul.Properties.Resources", Assembly.GetExecutingAssembly());

		public static OverhaulMod Instance { get; private set; }

		//These 2 properties are read by ModHelpers.
		public static string GithubUserName => "Mirsario";
		public static string GithubProjectName => "TerrariaOverhaul";

		static OverhaulMod()
		{
			Assembly = Assembly.GetExecutingAssembly();
			AssemblyTypes = Assembly.GetTypes();
			EngineAssembly = typeof(Game).Assembly;
			TMLAssembly = typeof(ModLoader).Assembly;
		}

		public OverhaulMod()
		{
			Instance = this;

			Directory.CreateDirectory(PersonalDirectory);

			/*if(ModLoader.version < MinimalTMLVersion) {
				throw new OutdatedTModLoaderException(MinimalTMLVersion);
			}*/

			Properties = new ModProperties {
				Autoload = true,
				AutoloadGores = false,
				AutoloadSounds = true,
				AutoloadBackgrounds = false
			};
		}

		public override void HandlePacket(BinaryReader reader, int sender) => MultiplayerSystem.HandlePacket(reader, sender);
	}
}
