using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Content.Items.Accessories;

[AutoloadEquip(EquipType.Neck)]
public class BunnyPaw : ModItem
{
	public override void SetDefaults()
	{
		// Accessory properties.
		Item.accessory = true;
		// Universal properties.
		Item.width = 32;
		Item.height = 32;
		Item.rare = ItemRarityID.Green;
		Item.scale = 0.25f;
		Item.value = Item.sellPrice(0, 0, 1);
	}

	public override void AddRecipes() => this.CreateRecipe(r => {
		r.AddIngredient(ItemID.Bunny, 5);
		r.AddTile(TileID.Sawmill);
	});

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		if (!player.TryGetModPlayer(out PlayerBunnyhopCombos bunnyhopCombos)) {
			return;
		}

		bunnyhopCombos.BoostBonusPerCombo += 0.0333f;
	}
}
