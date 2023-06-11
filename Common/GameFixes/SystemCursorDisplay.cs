using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Common.GameFixes;

/// <summary>
/// A simple tweak that makes the Operational System's cursor be displayed whenever UI is toggled off.
/// </summary>
public sealed class SystemCursorDisplay : ILoadable
{
	public static readonly ConfigEntry<bool> DisplaySystemCursorWithDisabledUI = new(ConfigSide.ClientOnly, "Tweaks", nameof(DisplaySystemCursorWithDisabledUI), () => true);
	
	void ILoadable.Load(Mod mod)
	{
		Main.QueueMainThreadAction(() => IL_Main.DoUpdate += Injection);
	}

	void ILoadable.Unload() { }

	private static void Injection(ILContext context)
	{
		var il = new ILCursor(context);

		// Match 'IsMouseVisible = false;'.
		if (!il.TryGotoNext(
			MoveType.Before,
			i => i.MatchLdarg(0),
			i => i.MatchLdcI4(0),
			i => i.MatchCall(typeof(Game), "set_IsMouseVisible")
		)) {
			// Not that important.
			DebugSystem.Logger.Warn($"{nameof(SystemCursorDisplay)}'s injection failed.");
			return;
		}

		il.Index += 2;

		il.Emit(OpCodes.Pop); // Pop the zero
		il.EmitDelegate(ShouldDisplayMouseCursor);
	}

	private static bool ShouldDisplayMouseCursor()
	{
		return Main.hideUI && !Main.gameMenu && DisplaySystemCursorWithDisabledUI;
	}
}
