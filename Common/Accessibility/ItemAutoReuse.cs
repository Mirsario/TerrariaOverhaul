using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Accessibility;

// Before Terraria 1.4.4 added global auto-reuse as an opt-in for players,
// Overhaul used to modify Item.autoReuse in SetDefaults() and add a small speed penalty to items it forced auto-reuse for:
//	if (ForceAutoReuse && (!item.autoReuse && !item.channel) {
//		item.useTime += 2;
//		item.useAnimation += 2;
//	}
// Nowadays we just override 'Main.SettingsEnabled_AutoReuseAllItems' so that the option is enabled by default.
public sealed class AutoReuseOverrideSystem : ModSystem
{
	public static readonly ConfigEntry<bool> ForceAutoReuse = new(ConfigSide.Both, "Accessibility", nameof(ForceAutoReuse), () => true);

	public override void PreUpdatePlayers()
	{
		Main.SettingsEnabled_AutoReuseAllItems |= ForceAutoReuse;
	}
}
