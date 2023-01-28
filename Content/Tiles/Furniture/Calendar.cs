using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Content.Tiles.Furniture;

public class Calendar : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileNoAttach[Type] = false;
		Main.tileLavaDeath[Type] = false;
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		this.AddTileObjectData(TileObjectData.Style3x3Wall, n => {
			n.Width = 2;
			n.Height = 2;
			n.Origin = new Point16(0, 0);
			n.CoordinateWidth = 16;
			n.CoordinateHeights = new[] { 16, 16 };
			n.CoordinatePadding = 2;

			n.StyleHorizontal = true;
			n.LavaDeath = true;
		});

		AddMapEntry(Color.IndianRed, CreateMapEntryName());
	}

	/*public override bool RightClick(int x, int y)
	{
		int days = SeasonSystem.SeasonLength - SeasonSystem.currentSeasonDay;

		// It's currently X...
		Main.NewText(LocalizationSystem.GetTextFormatted("SeasonSystem.CalendarCurrentSeason", SeasonSystem.currentSeason.DisplayName), Color.Yellow);

		// X will arrive in Y...
		Main.NewText(LocalizationSystem.GetTextFormatted($"SeasonSystem.{(days > 1 ? "CalendarNextSeasonDays" : "CalendarNextSeasonTomorrow")}", SeasonSystem.NextSeason.DisplayName, days), Color.Yellow);

		var player = Main.LocalPlayer;
		player.tileInteractAttempted = player.tileInteractionHappened = true;

		return true;
	}*/

	public override void MouseOver(int x, int y)
	{
		Main.cursorOverride = 3;
	}

	public override void KillMultiTile(int x, int y, int frameX, int frameY)
	{
		Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 32, 80, ModContent.ItemType<Items.Placeables.Calendar>());
	}
}
