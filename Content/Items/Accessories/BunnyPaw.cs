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
			item.accessory = true;
			//Universal properties.
			item.width = 32;
			item.height = 32;
			item.rare = ItemRarityID.Green;
			item.scale = 0.25f;
			item.value = Item.sellPrice(0, 0, 1);
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
