using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls;

public partial class Bow : ItemOverhaul
{
	public static readonly SoundStyle BowFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowFire", 4) {
		Volume = 0.375f,
		PitchVariance = 0.2f
	};
	public static readonly SoundStyle BowChargeSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowCharge", 4) {
		Volume = 0.375f,
		PitchVariance = 0.2f
	};
	public static readonly SoundStyle BowEmptySound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowEmpty") {
		Volume = 0.375f,
		PitchVariance = 0.2f
	};

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

			item.EnableComponent<ItemCrosshairController>();
		}
	}
}
