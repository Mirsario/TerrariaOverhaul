using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.GameFixes;

// Quite specific.
// Prevents other mods from accidentally calling calling Main.ResetGameCounter during gameplay.
public sealed class PreventUpdateCountResets : ILoadable
{
	public void Load(Mod mod)
	{
		On_Main.ResetGameCounter += (orig) => {
			if (!Main.LocalPlayer.active) {
				orig();
			}
		};
	}

	public void Unload() { }
}
