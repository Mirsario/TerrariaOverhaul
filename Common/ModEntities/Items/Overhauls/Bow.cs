using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public partial class Bow : ItemOverhaul, IShowItemCrosshair
	{
		public static readonly ModSoundStyle BowFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowFire", 4, volume: 0.375f, pitchVariance: 0.2f);
		public static readonly ModSoundStyle BowChargeSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowCharge", 4, volume: 0.375f, pitchVariance: 0.2f);
		public static readonly ModSoundStyle BowEmptySound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowEmpty", volume: 0.375f, pitchVariance: 0.2f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			// Ignore weapons that don't shoot, and ones that deal hitbox damage 
			if (item.shoot <= ProjectileID.None || !item.noMelee) {
				return false;
			}

			// Ignore weapons that don't shoot arrows.
			if (item.useAmmo != AmmoID.Arrow) {
				return false;
			}

			// Avoid tools and placeables
			if (item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			if (item.UseSound == SoundID.Item5) {
				item.UseSound = BowFireSound;
			}

			if (!Main.dedServ) {
				item.EnableComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(2f, 0.2f);
				});
			}
		}

		public bool ShowItemCrosshair(Item item, Player player) => true;
	}
}
