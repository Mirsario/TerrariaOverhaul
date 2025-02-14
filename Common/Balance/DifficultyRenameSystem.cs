// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.Balance;

// So convoluted.
internal sealed class DifficultyRenameSystem : ModSystem
{
	private const string KeyVanilla = "UI";
	private const string KeyMod = $"Mods.{nameof(TerrariaOverhaul)}.DifficultyLevels";

	private static readonly Dictionary<string, string> textBackups = new();
	private static readonly string[] keys = {
		"Creative",
		"Normal",
		"Expert",
		"Master",
		"WorldDescriptionCreative",
		"WorldDescriptionNormal",
		"WorldDescriptionExpert",
		"WorldDescriptionMaster",
	};
	private static bool isEnabled;

	private static bool ShouldBeEnabled => DifficultyRebalanceSystem.EnableDifficultyChanges;

	public override void Load()
	{
		On_LanguageManager.LoadFilesForCulture += LoadFilesForCulture;
	}

	public override void Unload()
	{
		Main.OnPreDraw -= OnPreDraw;

		if (isEnabled) {
			Undo();
		}

		textBackups.Clear();
	}

	public override void PostSetupContent()
	{
		Backup();

		Main.OnPreDraw += OnPreDraw;

		if (ShouldBeEnabled) {
			Apply();
		}
	}

	private static void OnPreDraw(GameTime gameTime)
	{
		if (isEnabled != ShouldBeEnabled) {
			if (!isEnabled) {
				Apply();
			} else {
				Undo();
			}
		}
	}

	private static void Backup()
	{
		foreach (string key in keys) {
			textBackups[key] = Language.GetTextValue($"{KeyVanilla}.{key}");
		}
	}

	private static void Apply()
	{
		foreach (string key in keys) {
			TextSystem.SetLocalizedTextValue(Language.GetText($"{KeyVanilla}.{key}"), Language.GetTextValue($"{KeyMod}.{key}"));
		}

		isEnabled = true;
	}

	private static void Undo()
	{
		foreach (string key in keys) {
			if (textBackups.TryGetValue(key, out string? value)) {
				TextSystem.SetLocalizedTextValue(Language.GetText($"{KeyVanilla}.{key}"), value);
			}
		}

		isEnabled = false;
	}

	private static void LoadFilesForCulture(On_LanguageManager.orig_LoadFilesForCulture orig, LanguageManager manager, GameCulture culture)
	{
		orig(manager, culture);

		Backup();
	}
}
