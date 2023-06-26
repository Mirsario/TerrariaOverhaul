using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Utilities;

public static class EntityExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Rectangle GetRectangle(this Entity entity)
	{
		return new Rectangle((int)entity.position.X, (int)entity.position.Y, entity.width, entity.height);
	}
}
