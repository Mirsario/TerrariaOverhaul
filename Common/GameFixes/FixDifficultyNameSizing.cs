using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Common.GameFixes
{
	// Very much not ideal.
	public sealed class FixDifficultyNameSizing : ILoadable
	{
		void ILoadable.Load(Mod mod)
		{
			var worldDifficultyIdEnumType = typeof(UIWorldCreation).GetNestedType("WorldDifficultyId", BindingFlags.NonPublic | BindingFlags.Public);

			if (worldDifficultyIdEnumType == null) {
				LogFailure($"Could not get a value for '{nameof(worldDifficultyIdEnumType)}'.");
				return;
			}

			IL.Terraria.GameContent.UI.States.UIWorldCreation.AddWorldDifficultyOptions += context => {
				var il = new ILCursor(context);

				int titleArrayLocalId = -1;
				int titleArrayIndexLocalId = -1;

				if (!il.TryGotoNext(
					MoveType.After,
					//T option
					i => i.MatchLdloc(out _),
					i => i.MatchLdloc(out _),
					i => i.MatchLdelemI4(),
					//LocalizedText title
					i => i.MatchLdloc(out titleArrayLocalId),
					i => i.MatchLdloc(out titleArrayIndexLocalId),
					i => i.MatchLdelemRef(),
					//LocalizedText description
					i => i.MatchLdloc(out _),
					i => i.MatchLdloc(out _),
					i => i.MatchLdelemRef(),
					//Color textColor
					i => i.MatchLdloc(out _),
					i => i.MatchLdloc(out _),
					i => i.MatchLdelemAny<Color>(),
					//string iconTexturePath
					i => i.MatchLdloc(out _),
					i => i.MatchLdloc(out _),
					i => i.MatchLdelemRef(),
					//float textSize = 1f
					i => i.MatchLdcR4(1f),
					//float titleAlignmentX
					i => i.MatchLdcR4(1f),
					//float titleWidthReduction
					i => i.MatchLdcR4(16f),
					// Constructor call
					i => i.MatchNewobj(typeof(GroupOptionButton<>).MakeGenericType(worldDifficultyIdEnumType))
				)) {
					LogFailure("Call #1 (TryGotoNext) failed.");
					return;
				}

				if (!il.TryGotoPrev(
					MoveType.Before,
					//float textSize = 1f
					i => i.MatchLdcR4(1f),
					//float titleAlignmentX
					i => i.MatchLdcR4(1f)
				)) {
					LogFailure("Call #2 (TryGotoPrev) failed.");
					return;
				}

				il.Remove();
				
				il.Emit(OpCodes.Ldloc, titleArrayLocalId);
				il.Emit(OpCodes.Ldloc, titleArrayIndexLocalId);
				il.Emit(OpCodes.Ldelem_Any, typeof(LocalizedText));

				il.EmitDelegate<Func<LocalizedText, float>>(localizedText => {
					float textWidth = FontAssets.MouseText.Value.MeasureString(localizedText.Value).X;
					float textScale = Math.Min(86f / Math.Max(textWidth, 1f), 1f);

					return textScale;
				});
			};
		}
		
		void ILoadable.Unload() { }

		private static void LogFailure(string details)
		{
			DebugSystem.Logger.Warn($"Fix '{nameof(FixDifficultyNameSizing)}' failed: {details}");
		}
	}
}
