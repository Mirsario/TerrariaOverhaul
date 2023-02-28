using System;
using System.Linq;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Localization;

// Temporary modifications to TML localization auto-population code.
internal sealed class LocalizationTweaks : ILoadable
{
	private delegate void DetourOriginal(Mod mod, string outputPath, GameCulture specificCulture);
	private delegate void DetourDelegate(DetourOriginal original, Mod mod, string outputPath, GameCulture specificCulture);

	[ThreadStatic]
	private static int modLocalizationCalls;

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
					HookEndpointManager.Add(updateLocalizationFilesForModMethod, (DetourDelegate)UpdateLocalizationFilesForModDetour);
					HookEndpointManager.Modify(localizationFileToHjsonText, (ILContext.Manipulator)LocalizationFileToHjsonTextInjection);
					HookEndpointManager.Modify(writeStringMethod, (ILContext.Manipulator)WriteStringInjection);
					return;
				}
			}
		}

		OverhaulMod.Instance.Logger.Warn($"{nameof(LocalizationTweaks)}: Methods are not found or differ.");
	}

	void ILoadable.Unload() { }

	private static void UpdateLocalizationFilesForModDetour(DetourOriginal original, Mod mod, string outputPath, GameCulture specificCulture)
	{
		int offset = mod.Name == nameof(TerrariaOverhaul) ? 1 : 0;

		try {
			modLocalizationCalls += offset;

			original(mod, outputPath, specificCulture);
		}
		finally {
			modLocalizationCalls -= offset;
		}
	}

	private static void LocalizationFileToHjsonTextInjection(ILContext context)
	{
		var il = new ILCursor(context);
		ILLabel? skipLabel = null;

		var instructionsA = new Func<Instruction, bool>[] {
			i => i.MatchLdstr("."),
			i => i.MatchLdloc(out _),
			i => i.MatchLdloc(out _),
			i => i.MatchCall(typeof(Enumerable).GetMethod(nameof(Enumerable.Skip))!.MakeGenericMethod(typeof(string))),
		};

		var instructionsB = new Func<Instruction, bool>[] {
			i => i.MatchBgt(out skipLabel),
		};

		if (!il.TryGotoNext(MoveType.Before, instructionsA) || !il.TryGotoPrev(MoveType.After, instructionsB)) {
			OverhaulMod.Instance.Logger.Warn($"{nameof(LocalizationTweaks)}/{nameof(LocalizationFileToHjsonTextInjection)}: Injection failed.");
			return;
		}

		il.EmitDelegate(static () => modLocalizationCalls > 0);
		il.Emit(OpCodes.Brtrue, skipLabel!);
	}

	private static void WriteStringInjection(ILContext context)
	{
		var il = new ILCursor(context);
		ILLabel? enforceQuotermarksLabel = null;

		if (!il.TryGotoNext(
			MoveType.After,
			i => i.MatchLdarg(3),
			i => i.MatchBrtrue(out enforceQuotermarksLabel)
		)) {
			OverhaulMod.Instance.Logger.Warn($"{nameof(LocalizationTweaks)}/{nameof(WriteStringInjection)}: Injection failed.");
			return;
		}

		il.EmitDelegate(static () => modLocalizationCalls > 0);
		il.Emit(OpCodes.Brtrue, enforceQuotermarksLabel!);
	}
}
