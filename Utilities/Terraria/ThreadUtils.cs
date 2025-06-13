using System;
using Terraria;

namespace TerrariaOverhaul.Utilities.Terraria;

public static class ThreadUtils
{
	public static void RunOnMainThread(Action action)
	{
		if (Program.IsMainThread) {
			action();
		} else {
			Main.RunOnMainThread(action).Wait();
		}
	}
}
