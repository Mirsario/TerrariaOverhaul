using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Systems.TileOverlays
{
	public sealed class TileOverlayTesting : ModPlayer
	{
		public override void PreUpdate()
		{
			if (Main.keyState.IsKeyDown(Keys.J)) {
				var tilePos = Main.MouseWorld.ToTileCoordinates();

				if (Main.tile.TryGet(tilePos, out var tile)) {
					ref var tileOverlayData = ref tile.Get<TileOverlayData>();

					tileOverlayData.OverlayType = (byte)1;
				}
			}
		}
	}
}
