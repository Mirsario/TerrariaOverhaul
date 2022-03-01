using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Networking;

namespace TerrariaOverhaul
{
	public partial class OverhaulMod : Mod
	{
		public static readonly uint BetaNumber = 9;
		public static readonly bool IsBeta = BetaNumber > 0;
		public static readonly string VersionSuffix = $"(BETA {BetaNumber} - FIX 1)";
		public static readonly string PersonalDirectory = Path.Combine(Main.SavePath, "TerrariaOverhaul");
		public static readonly Version MinimalTMLVersion = new("0.12");
		public static readonly Assembly Assembly;
		public static readonly Assembly EngineAssembly;
		public static readonly Assembly TMLAssembly;

		//internal static readonly ResourceManager ResourceManager = new ResourceManager("TerrariaOverhaul.Properties.Resources", Assembly.GetExecutingAssembly());

		public static OverhaulMod Instance { get; private set; }

		static OverhaulMod()
		{
			Assembly = Assembly.GetExecutingAssembly();
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
		}

		public override void HandlePacket(BinaryReader reader, int sender)
			=> MultiplayerSystem.HandlePacket(reader, sender);
	}
}
