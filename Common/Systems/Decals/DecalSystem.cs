using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Systems.Chunks;
using TerrariaOverhaul.Core.Systems.Configuration;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Systems.Decals
{
	[Autoload(Side = ModSide.Client)]
	public sealed class DecalSystem : ModSystem
	{
		public static readonly BlendState DefaultBlendState = BlendState.AlphaBlend;
		public static readonly ConfigEntry<bool> EnableDecals = new(ConfigSide.ClientOnly, "BloodAndGore", nameof(EnableDecals), () => true);

		public static Asset<Effect> BloodShader { get; private set; }

		public override void Load()
		{
			BloodShader = Mod.GetEffect("Assets/Shaders/Blood");
		}
		
		public override void Unload()
		{
			BloodShader = null;
		}

		public static void ClearDecals(Rectangle dest)
			=> AddDecals(dest, Color.Transparent, true, BlendState.Opaque);
		public static void AddDecals(Rectangle dest, Color color, bool ifChunkExists = false, BlendState blendState = null)
			=> AddDecals(TextureAssets.BlackTile.Value, dest, color, ifChunkExists, blendState);

		public static void AddDecals(Vector2 point, Color color, bool ifChunkExists = false, BlendState blendState = null)
		{
			var tilePos = point.ToTileCoordinates();

			if(!tilePos.IsInWorld()) {
				return;
			}

			AddDecals(new Rectangle((int)(point.X / 2) * 2, (int)(point.Y / 2) * 2, 2, 2), color, ifChunkExists, blendState);
		}
		
		public static void AddDecals(Texture2D texture, Rectangle dest, Color color, bool ifChunkExists = false, BlendState blendState = null)
		{
			if(Main.dedServ || WorldGen.gen || WorldGen.IsGeneratingHardMode || !EnableDecals) { // || !ConfigSystem.local.Clientside.BloodAndGore.enableTileBlood) {
				return;
			}

			if(texture == null) {
				throw new ArgumentNullException(nameof(texture));
			}

			if(blendState == null) {
				blendState = DefaultBlendState;
			}

			var chunkStart = new Vector2Int(
				(int)(dest.X / 16f / Chunk.MaxChunkSize),
				(int)(dest.Y / 16f / Chunk.MaxChunkSize)
			);
			var chunkEnd = new Vector2Int(
				(int)(dest.Right / 16f / Chunk.MaxChunkSize),
				(int)(dest.Bottom / 16f / Chunk.MaxChunkSize)
			);

			//The provided rectangle will be split between chunks, possibly into multiple draws.
			for(int chunkY = chunkStart.Y; chunkY <= chunkEnd.Y; chunkY++) {
				for(int chunkX = chunkStart.X; chunkX <= chunkEnd.X; chunkX++) {
					var chunkPoint = new Vector2Int(chunkX, chunkY);

					Chunk chunk;

					if(!ifChunkExists) {
						chunk = ChunkSystem.GetOrCreateChunk(chunkPoint);
					} else if(!ChunkSystem.TryGetChunk(chunkPoint, out chunk)) {
						continue;
					}

					var localDestRect = (RectFloat)dest;

					//Clip the destination rectangle to the chunk's bounds.
					localDestRect = RectFloat.FromPoints(
						Math.Max(localDestRect.x, chunk.WorldRectangle.x),
						Math.Max(localDestRect.y, chunk.WorldRectangle.y),
						Math.Min(localDestRect.Right, chunk.WorldRectangle.Right),
						Math.Min(localDestRect.Bottom, chunk.WorldRectangle.Bottom)
					);

					//Move the destination rectangle to local space.
					localDestRect.x -= chunk.WorldRectangle.x;
					localDestRect.y -= chunk.WorldRectangle.y;
					//Divide the destination rectangle, since decal RTs have halved resolution.
					localDestRect.x /= 2;
					localDestRect.y /= 2;
					localDestRect.width /= 2;
					localDestRect.height /= 2;

					//Clip the source rectangle.
					var destinationRectInChunkSpace = RectFloat.FromPoints(((RectFloat)dest).Points / Chunk.MaxChunkSizeInPixels);
					var clippedRectInChunkSpace = RectFloat.FromPoints(
						Math.Max(destinationRectInChunkSpace.Left, chunk.Rectangle.Left),
						Math.Max(destinationRectInChunkSpace.Top, chunk.Rectangle.Top),
						Math.Min(destinationRectInChunkSpace.Right, chunk.Rectangle.Right),
						Math.Min(destinationRectInChunkSpace.Bottom, chunk.Rectangle.Bottom)
					);

					var srcRect = (Rectangle)new RectFloat(
						(clippedRectInChunkSpace.x - destinationRectInChunkSpace.x) * (chunk.WorldRectangle.width / dest.Width) * texture.Width,
						(clippedRectInChunkSpace.y - destinationRectInChunkSpace.y) * (chunk.WorldRectangle.height / dest.Height) * texture.Height,
						(clippedRectInChunkSpace.width / destinationRectInChunkSpace.width) * texture.Width,
						(clippedRectInChunkSpace.height / destinationRectInChunkSpace.height) * texture.Height
					);

					//Enqueue a draw for the chunk component to do on its own.
					chunk.Components.Get<ChunkDecals>().AddDecals(texture, (Rectangle)localDestRect, srcRect, color, blendState);
				}
			}
		}
	}
}
