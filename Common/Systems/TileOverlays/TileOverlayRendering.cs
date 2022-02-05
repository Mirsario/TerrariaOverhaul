using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Systems.TileOverlays
{
	[Autoload(Side = ModSide.Client)]
	internal sealed class TileOverlayRendering : GlobalTile
	{
		private static Asset<Texture2D>[] snowTextures;

		public override void Load()
		{
			snowTextures = new[] {
				Mod.Assets.Request<Texture2D>("Common/Systems/TileOverlays/Snow_0"),
				Mod.Assets.Request<Texture2D>("Common/Systems/TileOverlays/Snow_1"),
				Mod.Assets.Request<Texture2D>("Common/Systems/TileOverlays/Snow_2"),
			};
		}

		public override void Unload()
		{
			base.Unload();
		}

		public override void PostDraw(int x, int y, int type, SpriteBatch spriteBatch)
		{
			var tile = Main.tile[x, y];
			ref readonly var tileOverlayData = ref tile.Get<TileOverlayData>();

			if (tileOverlayData.OverlayType == 0) {
				return;
			}

			var pos = new Point16(((x * 16) - (int)Main.screenPosition.X) + Main.offScreenRange, ((y * 16) - (int)Main.screenPosition.Y) + Main.offScreenRange);
			var blockType = tile.BlockType;

			Rectangle dstRect = new Rectangle(pos.X, pos.Y - 2, 16, 16);

			Rectangle srcRect;
			int overlaySubType;

			if (blockType <= BlockType.HalfBlock) {
				if (blockType == BlockType.HalfBlock) {
					dstRect.Y += 8;
				}

				srcRect = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);
				overlaySubType = TileID.Sets.Grass[tile.TileType] ? 1 : 0;
			} else {
				srcRect = new Rectangle((int)MathUtils.Modulo((tile.TileFrameX / 18f) * 16f, 48f), ((int)blockType - 1) * 16, 16, 16);
				overlaySubType = 2;
			}

			var texture = snowTextures[overlaySubType].Value;

			spriteBatch.Draw(texture, dstRect, srcRect, Terraria.Lighting.GetColor(x, y));
		}
	}

	/*
	public sealed class TileOverlayRenderingSystem : ModSystem
	{
		public override void PostDrawTiles()
		{
			
		}
	}
	*/
}
