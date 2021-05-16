using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Components;

namespace TerrariaOverhaul.Core.Systems.Chunks
{
	[GlobalComponent]
	public abstract class ChunkComponent : ModComponent<Chunk>
	{
		protected override void Register()
		{
			base.Register();

			ModTypeLookup<ChunkComponent>.Register(this);
		}

		public virtual void PreGameDraw(Chunk chunk) { }
		public virtual void PostDrawTiles(Chunk chunk, SpriteBatch sb) { }
	}
}
