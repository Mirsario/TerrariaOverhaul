// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

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
