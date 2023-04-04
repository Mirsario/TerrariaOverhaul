using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Bows;

public partial class Bow : ItemOverhaul
{
	public static readonly SoundStyle FireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowFire", 4) {
		Volume = 0.375f,
		PitchVariance = 0.2f,
		MaxInstances = 3,
	};
	public static readonly SoundStyle ChargeSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowCharge", 4) {
		Volume = 0.375f,
		PitchVariance = 0.2f,
		MaxInstances = 3,
	};
	public static readonly SoundStyle EmptySound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowEmpty") {
		Volume = 0.375f,
		PitchVariance = 0.2f,
		MaxInstances = 3,
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
			item.UseSound = FireSound;
		}

		item.EnableComponent<ItemPowerAttacks>(c => {
			c.CanRelease = true;
			c.ChargeLengthMultiplier = 2.0f;

			var weakest = new CommonStatModifiers {
				ProjectileSpeedMultiplier = 0.25f,
			};
			var strongest = new CommonStatModifiers {
				ProjectileDamageMultiplier = 2.0f,
				ProjectileKnockbackMultiplier = 2.0f,
				ProjectileSpeedMultiplier = 3.0f,
			};

			c.StatModifiers.Gradient = new(stackalloc Gradient<CommonStatModifiers>.Key[] {
				new(0.000f, weakest),
				new(0.750f, strongest),
				new(1.000f, strongest),
			});
		});

		if (!Main.dedServ) {
			item.EnableComponent<ItemPowerAttackSounds>(c => {
				c.Sound = ChargeSound;
				c.CancelPlaybackOnEnd = true;
			});
		}
	}
}
