using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Input
{
	public sealed class InputSystem : ModSystem
	{
		public static ModHotKey KeyReload { get; private set; }
		public static ModHotKey KeyDodgeroll { get; private set; }
		public static ModHotKey KeyEmoteWheel { get; private set; }
		public static ModHotKey KeyQuickAction { get; private set; }
		public static ModHotKey KeySwitchQuickItem { get; private set; }

		public override void Load()
		{
			KeyReload = Mod.RegisterHotKey("Reload","R");
			KeyDodgeroll = Mod.RegisterHotKey("Dodgeroll","LeftControl");
			KeyEmoteWheel = Mod.RegisterHotKey("Emote Wheel","Q");
			KeyQuickAction = Mod.RegisterHotKey("Quick Action","F");
			KeySwitchQuickItem = Mod.RegisterHotKey("Switch Quick Item","X");
		}
	}
}
