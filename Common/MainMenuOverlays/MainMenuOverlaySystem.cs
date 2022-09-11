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
using TerrariaOverhaul.Core.Localization;

namespace TerrariaOverhaul.Common.MainMenuOverlays;

[Autoload(Side = ModSide.Client)]
public sealed class MainMenuOverlaySystem : ModSystem
{
	private static DynamicSpriteFont Font => FontAssets.MouseText.Value;

	private static List<MenuLine>? menuLines;

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

			cursor.EmitDelegate(() => DrawOverlay(Main.spriteBatch));
		};

		const string MenuKey = "Mods.TerrariaOverhaul.MainMenu";
		const string MusicPackLink = @"https://steamcommunity.com/sharedfiles/filedetails/?id=2440081576";

		menuLines = new List<MenuLine> {
			// Version info
			new MenuLine(Text.Literal($"Terraria Overhaul v{OverhaulMod.Instance.Version} {OverhaulMod.VersionSuffix}")),

			// Patreon button
			new PatreonMenuLink(Text.Literal("patreon.com/Mirsario"), @"https://patreon.com/Mirsario") {
				ForcedColor = isHovered => Color.Lerp(Color.White, Main.DiscoColor, isHovered ? 0.5f : 0.75f)
			},
			
			// Configuration button
			new ConfigurationMenuButton(Text.Localized($"{MenuKey}.Configuration")),
			
			// Music Pack buttons
			new MusicPackMenuButton(Text.Localized($"{MenuKey}.MusicPack.Get"), Text.Localized($"{MenuKey}.MusicPack.Enable"), MusicPackLink) {
				PreferSteamBrowser = true,
				ForcedColor = isHovered => Color.Lerp(Color.White, Main.DiscoColor, isHovered ? 0.25f : 0.33f)
			},
			
			// Discord server
			new MenuLink(Text.Localized($"{MenuKey}.DiscordServer"), @"https://discord.gg/RNGq9N8"),

			// Forum page -- Hidden for now.
			//new MenuLink(Text.Localized($"{MenuKey}.ForumPage"), @"https://forums.terraria.org/index.php?threads/60369"),
			
			// Wiki page
			new MenuLink(Text.Localized($"{MenuKey}.WikiPage"), @"https://terrariamods.gamepedia.com/Terraria_Overhaul"),

			// Github page
			new MenuLink(Text.Localized($"{MenuKey}.Github"), @"https://github.com/Mirsario/TerrariaOverhaul"),
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
				if (!entry.IsActive) {
					continue;
				}
				
				entry.Update(textPos);
				entry.Draw(sb, textPos);

				textPos.Y += entry.Size.Y;
			}
		}

		sb.End();
	}
}
