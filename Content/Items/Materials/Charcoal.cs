using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerrariaOverhaul.Content.Items.Materials
{
	public class Charcoal : OverhaulItem
	{
		public override void SetDefaults()
		{
			item.CloneDefaults(ItemID.Gel);

			item.color = Color.White;
			item.maxStack = 999;
			item.ammo = 0;
		}

		/*protected void OverhaulInit() //TODO: Reimplement
		{
			item.SetTag(ItemTags.Flammable);
		}*/
	}
}