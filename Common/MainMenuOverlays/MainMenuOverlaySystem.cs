using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.MainMenuOverlays
{
	[Autoload(Side = ModSide.Client)]
	public sealed class MainMenuOverlaySystem : ModSystem
	{
		private static DynamicSpriteFont Font => FontAssets.MouseText.Value;

		private static List<MenuLine> menuLines;

		public override void Load()
		{
			// Draw the overlay right before the cursor.
			IL.Terraria.Main.DrawMenu += context => {
				var cursor = new ILCursor(context);

				//Source:
				//	spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, UIScaleMatrix);

				if (!cursor.TryGotoNext(
					i => i.MatchLdsfld(typeof(Main), nameof(Main.spriteBatch)),
					i => i.Match(OpCodes.Ldc_I4_0),
					i => i.MatchLdsfld(typeof(BlendState), nameof(BlendState.AlphaBlend)),
					i => i.MatchLdsfld(typeof(Main), nameof(Main.SamplerStateForCursor)),
					i => i.MatchLdsfld(typeof(DepthStencilState), nameof(DepthStencilState.None)),
					i => i.MatchLdsfld(typeof(RasterizerState), nameof(RasterizerState.CullCounterClockwise)),
					i => i.Match(OpCodes.Ldnull),
					i => i.MatchCall(typeof(Main), $"get_{nameof(Main.UIScaleMatrix)}")
				)) {
					throw new Exception($"{nameof(MainMenuOverlaySystem)}: IL Failure.");
				}

				cursor.EmitDelegate<Action>(() => DrawOverlay(Main.spriteBatch));
			};

			menuLines = new List<MenuLine> {
				new MenuLine($"Terraria Overhaul v{OverhaulMod.Instance.Version} {OverhaulMod.VersionSuffix}"),
				new PatreonMenuLink("patreon.com/Mirsario", @"https://patreon.com/Mirsario", forcedColor: _ => Main.DiscoColor),
				new MenuLink("Discord Server", @"https://discord.gg/RNGq9n8"),
				new MenuLink("Forum Page", @"https://forums.terraria.org/index.php?threads/60369"),
				new MenuLink("Wiki Page", @"https://terrariamods.gamepedia.com/Terraria_Overhaul"),
				new MenuLink("Github", @"https://github.com/Mirsario/TerrariaOverhaul/tree/1.4"),
			};
		}

		public override void Unload()
		{
			menuLines = null;
		}

		private static void DrawOverlay(SpriteBatch sb)
		{
			if (!Main.gameMenu) {
				return;
			}

			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

			var textPos = new Vector2(16, 16);
			var lines = menuLines; // Copy for thread safety.

			if (lines != null) {
				foreach (var entry in lines) {
					entry.Update(textPos);
					entry.Draw(sb, textPos);

					textPos.Y += entry.Size.Y;
				}
			}

			sb.End();
		}
	}
}
