// Copyright (c) 2020-2024 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using MonoMod.Cil;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Localization;

public sealed class TextSystem : ModSystem
{
	private delegate List<(string, string)> LoadTranslationsDelegate(Mod mod, GameCulture culture);
	
	private static bool isLoaded;
	private static bool forcedLocalizationLoad;
	private static LoadTranslationsDelegate? loadTranslations;
	private static int languageRefreshCount;

	internal static int LanguageRefreshCount => languageRefreshCount;
	private static Mod ModInstance => OverhaulMod.Instance;

	public override void Load()
	{
		EnsureInitialized();
	}

	private static void EnsureInitialized()
	{
		if (isLoaded) {
			return;
		}

		var flags = ReflectionUtils.AnyBindingFlags;

		if (typeof(LocalizationLoader).GetMethod("LoadModTranslations", flags) is MethodInfo loadModTranslations) {
			MonoModHooks.Modify(loadModTranslations, InjectLanguageRefreshCounter);
		} else {
			DebugSystem.Logger.Error($"{nameof(TextSystem)}: Failed to inject a language refresh counter.");
		}

		var loadTranslationsMethod = typeof(LocalizationLoader).GetMethod("LoadTranslations", flags, new[] { typeof(Mod), typeof(GameCulture) });
		var localizedTextSetValueMethod = typeof(LocalizedText).GetMethod("SetValue", flags, new[] { typeof(string) });

		if (loadTranslationsMethod != null && loadTranslationsMethod.ReturnType == typeof(List<(string, string)>)) {
			loadTranslations = loadTranslationsMethod.CreateDelegate<LoadTranslationsDelegate>();
		} else {
			DebugSystem.Logger.Error($"{nameof(TextSystem)}: Failed to acquire LoadModTranslations method.");
		}

		isLoaded = true;
	}

	[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "SetValue")]
	public static extern void SetLocalizedTextValue(LocalizedText text, string value);

	public static string GetTextValueSafe(string key)
	{
		string result = Language.GetTextValue(key);

		if (result == key) {
			if (ForceInitializeLocalization()) {
				result = Language.GetTextValue(key);
			}
		}

		return result;
	}

	public static bool ForceInitializeLocalization()
	{
		if (LanguageRefreshCount != 0 || forcedLocalizationLoad) {
			return false;
		}

		EnsureInitialized();

		forcedLocalizationLoad = true;

		if (loadTranslations == null) {
			return false;
		}

		static void LoadForCulture(GameCulture culture)
		{
			var languageManager = LanguageManager.Instance;

			foreach (var (key, value) in loadTranslations!(ModInstance, Language.ActiveCulture)) {
				SetLocalizedTextValue(languageManager!.GetText(key), value);
			}
		}

		using (new Logging.QuietExceptionHandle()) {
			try {
				LoadForCulture(GameCulture.DefaultCulture);

				if (Language.ActiveCulture != GameCulture.DefaultCulture) {
					LoadForCulture(Language.ActiveCulture);
				}
			}
			catch { }
		}

		Interlocked.Increment(ref languageRefreshCount);

		return true;
	}

	private static void InjectLanguageRefreshCounter(ILContext context)
	{
		var il = new ILCursor(context);

		il.EmitDelegate(static () => {
			Interlocked.Increment(ref languageRefreshCount);
		});
	}
}
