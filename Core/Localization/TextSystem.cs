using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Core.Localization;

public sealed class TextSystem : ModSystem
{
	private delegate void RefreshModLanguageDelegate(GameCulture culture);
	private delegate void RefreshModLanguageHook(RefreshModLanguageDelegate original, GameCulture culture);

	internal static int LanguageRefreshCount { get; private set; }

	public override void Load()
	{
		const string MethodName = nameof(LocalizationLoader.LoadModTranslations);

		var method = typeof(LocalizationLoader).GetMethod(MethodName, BindingFlags.Public | BindingFlags.Static);

		if (method == null) {
			DebugSystem.Logger.Error($"Unable to get the {MethodName} method! The localization system will suffer from this.");
			return;
		}

		HookEndpointManager.Add(method, (RefreshModLanguageHook)Hook);
	}

	private static void Hook(RefreshModLanguageDelegate original, GameCulture culture)
	{
		original(culture);

		LanguageRefreshCount++;
	}
}
