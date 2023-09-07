using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Localization;

// Temporary modifications to TML localization auto-population code.
internal sealed class LocalizationTweaks : ILoadable
{
	private delegate void DetourOriginal(Mod mod, string outputPath, GameCulture specificCulture);
	private delegate void DetourDelegate(DetourOriginal original, Mod mod, string outputPath, GameCulture specificCulture);

	[ThreadStatic]
	private static int personalModLocalizationCalls;

	void ILoadable.Load(Mod mod)
	{
		const BindingFlags BindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		var updateLocalizationFilesForModMethod = typeof(LocalizationLoader)
			.GetMethod("UpdateLocalizationFilesForMod", BindingFlags);

		var localizationFileToHjsonText = typeof(LocalizationLoader)
			.GetMethod("LocalizationFileToHjsonText", BindingFlags);

		var writeStringMethod = typeof(ModLoader).Assembly
			.GetType("Terraria.ModLoader.Utilities.HjsonExtensions")?
			.GetMethod("WriteString", BindingFlags);

		if (updateLocalizationFilesForModMethod != null && localizationFileToHjsonText != null && writeStringMethod != null) {
			var parametersA = updateLocalizationFilesForModMethod.GetParameters();
			var parametersB = typeof(DetourOriginal).GetMethod("Invoke")!.GetParameters();

			if (parametersA.Length == parametersB.Length) {
				bool parametersEqual = true;

				for (int i = 0; i < parametersA.Length; i++) {
					var (a, b) = (parametersA[i], parametersB[i]);

					if (a.ParameterType != b.ParameterType || a.IsOut != b.IsOut || a.IsIn != b.IsIn) {
						parametersEqual = false;
						break;
					}
				}

				if (parametersEqual) {
					MonoModHooks.Add(updateLocalizationFilesForModMethod, (DetourDelegate)UpdateLocalizationFilesForModDetour);
					MonoModHooks.Modify(localizationFileToHjsonText, LocalizationFileToHjsonTextInjection);
					MonoModHooks.Modify(writeStringMethod, WriteStringInjection);
					return;
				}
			}
		}

		OverhaulMod.Instance.Logger.Warn($"{nameof(LocalizationTweaks)}: Methods are not found or differ.");
	}

	void ILoadable.Unload() { }

	// Tracks localization updates for this mod.
	private static void UpdateLocalizationFilesForModDetour(DetourOriginal original, Mod mod, string outputPath, GameCulture specificCulture)
	{
		int offset = mod.Name == nameof(TerrariaOverhaul) ? 1 : 0;

		try {
			personalModLocalizationCalls += offset;

			original(mod, outputPath, specificCulture);
		}
		finally {
			personalModLocalizationCalls -= offset;
		}
	}

	// Disables "A: { B: C }" being converted to "A.B: C".
	private static void LocalizationFileToHjsonTextInjection(ILContext context)
	{
		var il = new ILCursor(context);
		ILLabel? skipLabel = null;

		var instructionsA = new Func<Instruction, bool>[] {
			i => i.MatchLdstr("."),
			i => i.MatchLdloc(out _),
			i => i.MatchLdloc(out _),
			i => i.MatchCall(typeof(Enumerable).GetMethod(nameof(Enumerable.Skip))!.MakeGenericMethod(typeof(string))),
			i => i.MatchCall(typeof(string).GetMethod(nameof(string.Join), new[] { typeof(string), typeof(IEnumerable<>).MakeGenericType(typeof(string)) })!),
		};

		var instructionsB = new Func<Instruction, bool>[] {
			i => i.MatchBrfalse(out skipLabel),
		};

		if (!il.TryGotoNext(MoveType.Before, instructionsA) || !il.TryGotoPrev(MoveType.After, instructionsB)) {
			OverhaulMod.Instance.Logger.Warn($"{nameof(LocalizationTweaks)}/{nameof(LocalizationFileToHjsonTextInjection)}: Injection failed.");
			return;
		}

		il.EmitDelegate(static () => personalModLocalizationCalls > 0);
		il.Emit(OpCodes.Brtrue, skipLabel!);
	}

	// Forces strings to always use quotemarks.
	private static void WriteStringInjection(ILContext context)
	{
		const string HasCommentParameter = "hasComment";
		int hasCommentIndex = context.Method.Parameters.FirstOrDefault(p => p.Name == HasCommentParameter)?.Index ?? -1;

		if (hasCommentIndex == -1) {
			OverhaulMod.Instance.Logger.Warn($"{nameof(LocalizationTweaks)}/{nameof(WriteStringInjection)}: No '{HasCommentParameter}' parameter.");
			return;
		}
		
		var il = new ILCursor(context);
		ILLabel? placeQuoteWrappedStringLabel = null;

		if (!il.TryGotoNext(
			MoveType.After,
			i => i.MatchLdarg(hasCommentIndex),
			i => i.MatchBrtrue(out placeQuoteWrappedStringLabel)
		)) {
			OverhaulMod.Instance.Logger.Warn($"{nameof(LocalizationTweaks)}/{nameof(WriteStringInjection)}: Injection failed.");
			return;
		}

		il.EmitDelegate(ShouldForceQuoteWrappingInHjson);
		il.Emit(OpCodes.Brtrue, placeQuoteWrappedStringLabel!);
	}

	private static bool ShouldForceQuoteWrappingInHjson()
	{
		return personalModLocalizationCalls > 0;
	}
}
