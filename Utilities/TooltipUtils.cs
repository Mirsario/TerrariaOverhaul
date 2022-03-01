using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Input;

namespace TerrariaOverhaul.Utilities
{
	public static class TooltipUtils
	{
		public static bool ShowCombatInformation(Mod mod, List<TooltipLine> tooltips, Func<IEnumerable<string>> getLines)
		{
			const Keys Key = Keys.LeftAlt;

			var color = Color.MediumVioletRed;

			void AddTooltip(string name, string text)
			{
				tooltips.Add(new TooltipLine(mod, name, text) {
					overrideColor = color
				});
			}

			if (InputSystem.GetKey(Key)) {
				var lines = getLines().Prepend(mod.GetTextValue("CommonTooltips.CombatInfo"));

				AddTooltip("CombatInfo", string.Join("\r\n", lines));

				return true;
			}

			AddTooltip("ShowCombatInfo", mod.GetTextValue("CommonTooltips.ShowCombatInfo").Replace("{Key}", Key.ToString()));

			return false;
		}
	}
}
