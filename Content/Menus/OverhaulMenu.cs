using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Menus
{
	[Autoload(Side = ModSide.Client)]
	public sealed class OverhaulMenu : ModMenu
	{
		private Asset<Texture2D>? logoTerraria;
		private Asset<Texture2D>? logoOverhaul;
		private Asset<Texture2D>? logoGlowmask;
		//private BlendState blendState;

		public override Asset<Texture2D>? Logo => logoOverhaul;

		public override void Load()
		{
			logoTerraria = Mod.Assets.Request<Texture2D>("Content/Menus/Logo_Terraria");
			logoOverhaul = Mod.Assets.Request<Texture2D>("Content/Menus/Logo_Overhaul");
			logoGlowmask = Mod.Assets.Request<Texture2D>("Content/Menus/Logo_Glowmask");
		}

		public override bool PreDrawLogo(SpriteBatch sb, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			if (logoOverhaul?.IsLoaded != true || logoTerraria?.IsLoaded != true || logoGlowmask?.IsLoaded != true) {
				return false;
			}

			var textureSize = logoOverhaul.Value.Size();
			var textureCenter = textureSize * 0.5f;

			// Small 'Terraria' in background
			sb.Draw(logoTerraria.Value, logoDrawCenter, null, drawColor, logoRotation, textureCenter, logoScale, SpriteEffects.None, 0f);

			// 'Overhaul' in foreground
			sb.Draw(logoOverhaul.Value, logoDrawCenter, null, drawColor, logoRotation, textureCenter, logoScale, SpriteEffects.None, 0f);

			// 'Overhaul' glowmask'
			sb.Draw(logoGlowmask.Value, logoDrawCenter, null, Color.White, logoRotation, textureCenter, logoScale, SpriteEffects.None, 0f);

			return false;
		}
	}
}
