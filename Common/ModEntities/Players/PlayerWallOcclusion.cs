using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Debugging;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerWallOcclusion : ModPlayer
	{
		public float OcclusionFactor { get; private set; }

		public override void PostUpdate()
		{
			if(!Player.IsLocal()) {
				return;
			}

			Vector2Int areaCenter = Player.Center.ToTileCoordinates();
			Vector2Int halfSize = new Vector2Int(5, 5);
			Vector2Int size = halfSize * 2;
			Vector2Int start = areaCenter - halfSize;
			Vector2Int end = areaCenter + halfSize;

			const float RequiredWallRatio = 0.4f;

			int maxTiles = size.X * size.Y;
			int requiredWallTiles = (int)(maxTiles * RequiredWallRatio);
			int numWalls = 0;

			GeometryUtils.FloodFill(
				areaCenter - start,
				size,
				(Vector2Int p, out bool occupied, ref bool stop) => {
					int x = p.X + start.X;
					int y = p.Y + start.Y;
					Tile tile = Main.tile[x, y];

					occupied = tile.IsActive && Main.tileSolid[tile.type] && !Main.tileSolidTop[tile.type] && tile.BlockType == Terraria.ID.BlockType.Solid;

					if(!occupied && (tile.wall > 0 || y >= Main.worldSurface)) {
						numWalls++;

						if(DebugSystem.EnableDebugRendering) {
							DebugSystem.DrawRectangle(new Rectangle(x * 16, y * 16, 16, 16), Color.Red, 1);
						}

						if(numWalls >= requiredWallTiles && !DebugSystem.EnableDebugRendering) {
							stop = true;
						}
					}
				}
			);

			OcclusionFactor = MathHelper.Clamp(numWalls / (float)requiredWallTiles, 0f, 1f);
		}
	}
}
