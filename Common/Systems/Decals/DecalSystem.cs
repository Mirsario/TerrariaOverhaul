using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Chunks;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Systems.Decals
{
	public sealed class DecalSystem : ModSystem
	{
		public static Asset<Effect> BloodShader { get; private set; }

		public override void Load()
		{
			BloodShader = Mod.GetEffect("Assets/Shaders/Blood");
		}
		public override void Unload()
		{
			BloodShader = null;
		}

		public static void AddDecals(Vector2 point, Color color, bool ifChunkExists = false, bool doSurroundedChecks = true)
		{
			var tilePos = point.ToTileCoordinates();

			if(!tilePos.IsInWorld()) {
				return;
			}

			/*if(!ifChunkExists && doSurroundedChecks && TileCheckUtils.CheckTotallySurrounded(tilePos.X, tilePos.Y)) {
				return;
			}*/

			AddDecals(new Rectangle((int)(point.X / 2) * 2, (int)(point.Y / 2) * 2, 2, 2), color, ifChunkExists);
		}
		public static void AddDecals(Rectangle rect, Color color, bool ifChunkExists = false)
		{
			if(Main.dedServ || WorldGen.gen || WorldGen.IsGeneratingHardMode) { // || !ConfigSystem.local.Clientside.BloodAndGore.enableTileBlood) {
				return;
			}

			//color = color.WithAlpha(255);

			var chunkStart = new Point(
				(int)Math.Floor(rect.X / 16f / Chunk.MaxChunkSize),
				(int)Math.Floor(rect.Y / 16f / Chunk.MaxChunkSize)
			);
			var chunkEnd = new Point(
				(int)Math.Ceiling(rect.Right / 16f / Chunk.MaxChunkSize),
				(int)Math.Ceiling(rect.Bottom / 16f / Chunk.MaxChunkSize)
			);

			//The provided rectangle will be split between chunks, possibly into multiple draws.
			for(int chunkY = chunkStart.Y; chunkY < chunkEnd.Y; chunkY++) {
				for(int chunkX = chunkStart.X; chunkX < chunkEnd.X; chunkX++) {
					var chunkPoint = new Point(chunkX, chunkY);

					Chunk chunk;

					if(!ifChunkExists) {
						chunk = ChunkSystem.GetOrCreateChunk(chunkPoint);
					} else if(!ChunkSystem.TryGetChunk(chunkPoint, out chunk)) {
						continue;
					}

					var localRectangle = rect;

					//Clip the rectangle to the chunk's bounds
					localRectangle = RectangleUtils.FromPoints(
						Math.Max(localRectangle.X, chunk.WorldRectangle.X),
						Math.Max(localRectangle.Y, chunk.WorldRectangle.Y),
						Math.Min(localRectangle.Right, chunk.WorldRectangle.Right),
						Math.Min(localRectangle.Bottom, chunk.WorldRectangle.Bottom)
					);

					//Move the rectangle to local space
					localRectangle.X -= chunk.WorldRectangle.X;
					localRectangle.Y -= chunk.WorldRectangle.Y;
					localRectangle.X /= 2;
					localRectangle.Y /= 2;
					localRectangle.Width /= 2;
					localRectangle.Height /= 2;

					//Enqueue a draw for the chunk component to do on its own.
					chunk.GetComponent<ChunkDecals>().AddDecals(localRectangle, color);
				}
			}
		}
	}
}
