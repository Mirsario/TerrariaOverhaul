using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Content.Items.Accessories
{
	public class BunnyPaw : ItemBase
	{
		public override void SetDefaults()
		{
			//Accessory properties.
			Item.accessory = true;
			//Universal properties.
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
		//TODO: Reimplement.
		/*public override void OnCraft(Recipe recipe)
		{
			if(!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.NPCDeath16,Main.LocalPlayer.Center);
				
				SoundInstance.Create<OggSoundInstance,OggSoundInfo>("Gore",Main.LocalPlayer.Center,0.5f,playDelay:0.375f);
				
				Main.NewText(LocalizationSystem.GetText("AdditionalTooltips.OnBunnyPawCraft"));
			}
		}*/
	}
}
