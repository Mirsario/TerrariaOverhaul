// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerrariaOverhaul.Utilities;

public static class ModTileExtensions
{
	public static void AddTileObjectData(this ModTile modTile, TileObjectData copyFrom, Action<TileObjectData> action)
	{
		TileObjectData.newTile.CopyFrom(copyFrom);

		action(TileObjectData.newTile);

		TileObjectData.addTile(modTile.Type);
	}

	public static void AddAlternate(this TileObjectData tileObjectData, int altStyleId, Action<TileObjectData> action)
	{
		TileObjectData.newAlternate.CopyFrom(tileObjectData);

		action(TileObjectData.newAlternate);

		TileObjectData.addAlternate(altStyleId);
	}
}
