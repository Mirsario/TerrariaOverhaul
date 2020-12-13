using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Chunks
{
	//TODO: This is temporary. ModWorld hooks need to be in ModSystem.
	public sealed class ChunkWorld : ModWorld
	{
		public override void PostDrawTiles() => ModContent.GetInstance<ChunkSystem>().PostDrawTiles();
	}
}
