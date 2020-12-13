using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Chunks
{
	public abstract class ChunkComponent : ModType, IDisposable
	{
		public int Id { get; private set; }
		public Chunk Chunk { get; private set; }

		protected override void Register()
		{
			ModTypeLookup<ChunkComponent>.Register(this);

			Id = ChunkSystem.RegisterComponent(this);
		}

		public virtual ChunkComponent Clone(Chunk chunk)
		{
			var result = (ChunkComponent)MemberwiseClone();

			result.Chunk = chunk;

			return result;
		}
		public virtual void OnInit() { }
		public virtual void OnDispose() { }
		//public virtual void OnUpdate() { }
		public virtual void PreGameDraw() { }
		public virtual void PostDrawTiles(SpriteBatch sb) { }

		public void Dispose()
		{
			OnDispose();

			Chunk = null;
		}
	}
}
