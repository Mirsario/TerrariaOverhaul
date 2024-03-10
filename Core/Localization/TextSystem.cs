using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Core.Localization;

public sealed class TextSystem : ModSystem
{
	//TODO: Use unsafe accessors in .NET 8.
	private delegate void LocalizedTextSetValueDelegate(LocalizedText text, string value);
	private delegate List<(string, string)> LoadTranslationsDelegate(Mod mod, GameCulture culture);

	private static bool isLoaded;
	private static bool forcedLocalizationLoad;
	private static LoadTranslationsDelegate? loadTranslations;
	private static LocalizedTextSetValueDelegate? localizedTextSetValue;

	internal static int LanguageRefreshCount { get; private set; }
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

		if (localizedTextSetValueMethod != null && localizedTextSetValueMethod.ReturnType == typeof(void)) {
			localizedTextSetValue = localizedTextSetValueMethod.CreateDelegate<LocalizedTextSetValueDelegate>();
		} else {
			DebugSystem.Logger.Error($"{nameof(TextSystem)}: Failed to acquire LocalizedText.SetValue method.");
		}

		isLoaded = true;
	}

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

		if (loadTranslations == null || localizedTextSetValue == null ) {
			return false;
		}

		static void LoadForCulture(GameCulture culture)
		{
			var languageManager = LanguageManager.Instance;

			foreach (var (key, value) in loadTranslations!(ModInstance, Language.ActiveCulture)) {
				localizedTextSetValue!(languageManager!.GetText(key), value);
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

		LanguageRefreshCount = 0;

		return true;
	}

	private static void InjectLanguageRefreshCounter(ILContext context)
	{
		var il = new ILCursor(context);

		il.EmitDelegate(static () => {
			LanguageRefreshCount++;
		});
	}
}
