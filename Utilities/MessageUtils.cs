using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities
{
	public static class MessageUtils
	{
		public static void NewText(object text, Color? color = null)
		{
			if(Main.dedServ) {
				Console.WriteLine(text);
			} else {
				Main.NewText(text, color);
			}
		}
	}
}
