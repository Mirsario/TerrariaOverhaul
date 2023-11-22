using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Utilities;

public static class ModExtensions
{
	public static string GetTextValue(this Mod mod, string key)
	{
		return Language.GetTextValue($"Mods.{mod.Name}.{key}");
	}

	public static string GetTextValue(this Mod mod, string key, params object[] args)
	{
		return Language.GetTextValue($"Mods.{mod.Name}.{key}", args);
	}
}
