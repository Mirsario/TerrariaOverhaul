// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.UI;
using TerrariaOverhaul.Common.ConfigurationScreen;
using TerrariaOverhaul.Content.Decals;
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.MainMenuOverlays;

internal class ConfigurationMenuButton : MenuButton
{
	public ConfigurationMenuButton(Text text) : base(text) { }

	protected override void OnClicked()
	{
		if (typeof(ModLoader).GetField("isLoading", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) is true) {
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		Main.MenuUI.SetState(ConfigurationState.Instance);
		Main.menuMode = 888;
	}
}

internal class Configuration : ModConfig // Adds Overhaul to the list of mod configs. The config button click event will open our custom menu
{
	public override ConfigScope Mode => ConfigScope.ClientSide;
}

internal sealed class ConfigurationMenuButton2 : ILoadable
{
	private static readonly Assembly assembly = typeof(ModConfig).Assembly;
	private readonly Type configButtonClickDelegateBodyType = assembly.GetType("Terraria.ModLoader.Config.UI.UIModConfigList+<>c__DisplayClass10_0")!;
	private readonly Type uIModItemConfigButtonClickDelegateBodyType = assembly.GetType("Terraria.ModLoader.UI.UIModItem")!;

	void ILoadable.Unload() { }
	void ILoadable.Load(Mod mod)
	{
		MonoModHooks.Add(configButtonClickDelegateBodyType!.GetMethod("<PopulateMods>b__2", BindingFlags.NonPublic | BindingFlags.Instance), ConfigButtonClick);
		MonoModHooks.Add(uIModItemConfigButtonClickDelegateBodyType!.GetMethod("OpenConfig", BindingFlags.NonPublic | BindingFlags.Instance)!, UIModItemConfigButtonClick);
	}

	delegate void orig_ConfigButtonClick(object self, UIMouseEvent evt, UIElement listeningElement);
	private static void ConfigButtonClick(orig_ConfigButtonClick orig, object self, UIMouseEvent evt, UIElement listeningElement)
	{
		// the compiler-generated delegate method
		var method = typeof(ModConfig).Assembly
			.GetTypes()
			.SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
			.FirstOrDefault(m => m.Name == "<PopulateMods>b__2");

		Mod? mod = (Mod?)self.GetType().GetField("mod")?.GetValue(self);

		if (mod is not OverhaulMod) {
			orig(self, evt, listeningElement);
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		if (Main.gameMenu) {
			Main.MenuUI.SetState(ConfigurationState.Instance);
			Main.menuMode = 888;
		} else IngameFancyUI.OpenUIState(ConfigurationState.Instance);
	}


	delegate void orig_UIModItemConfigButtonClick(object self, UIMouseEvent evt, UIElement listeningElement);
	private static void UIModItemConfigButtonClick(orig_UIModItemConfigButtonClick orig, object self, UIMouseEvent evt, UIElement listeningElement)
	{
		var prop = self.GetType().GetProperty("ModName");


		string? modName = (string?)prop?.GetGetMethod(true)?.Invoke(self, null);

		if (modName == null || modName != nameof(TerrariaOverhaul)) {
			orig(self, evt, listeningElement);
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		if (Main.gameMenu) {
			Main.MenuUI.SetState(ConfigurationState.Instance);
			Main.menuMode = 888;
		} else IngameFancyUI.OpenUIState(ConfigurationState.Instance);
	}
}
