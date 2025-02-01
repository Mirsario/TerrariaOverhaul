// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Components;

namespace TerrariaOverhaul.Core.Chunks;

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
