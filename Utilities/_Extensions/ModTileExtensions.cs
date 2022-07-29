using System;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerrariaOverhaul.Utilities;

public static class ModTileExtensions
{
	public static void AddMapEntry(this ModTile modTile, Color color, string name)
	{
		var entryName = modTile.CreateMapEntryName();

		entryName.SetDefault(name);
		modTile.AddMapEntry(color, entryName);
	}

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
