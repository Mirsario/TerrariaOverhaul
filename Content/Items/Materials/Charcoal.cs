using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerrariaOverhaul.Content.Items.Materials
{
	public class Charcoal : ItemBase
	{
		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.Gel);

			Item.color = Color.White;
			Item.maxStack = 999;
			Item.ammo = 0;
		}

		/*protected void OverhaulInit() //TODO: Reimplement
		{
			item.SetTag(ItemTags.Flammable);
		}*/
	}
}
