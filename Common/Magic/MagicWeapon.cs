using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Magic;

public partial class MagicWeapon : ItemOverhaul
{
	public static readonly SoundStyle MagicBlastSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicBlast", 3) {
		PitchVariance = 0.1f,
	};
	public static readonly SoundStyle MagicPowerfulBlastSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicPowerfulBlast") {
		PitchVariance = 0.4f,
	};
	public static readonly SoundStyle ChargeSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicCharge") {
		Volume = 0.5f,
		PitchVariance = 0.1f,
	};

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		// Ignore weapons with non-magic damage types
		if (!item.CountsAsClass(DamageClass.Magic)) {
			return false;
		}

		// Avoid tools and placeables
		if (item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
			return false;
		}

		// Ignore weapons that don't shoot, don't use mana, or deal hitbox damage 
		if (item.shoot <= ProjectileID.None || item.mana <= 0 || !item.noMelee) {
			return false;
		}

		// Ignore laser guns
		if (item.UseSound == SoundID.Item157) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		if (item.UseSound == SoundID.Item43) {
			item.UseSound = MagicBlastSound;
		}

		var chargeScreenShakePowerGradient = new Gradient<float>(
			(0.0f, 0.0f),
			(0.25f, 0.025f),
			(1.0f, 0.2f)
		);

		item.EnableComponent<ItemPowerAttacks>(c => {
			c.ChargeLengthMultiplier = 2f;

			var modifiers = new CommonStatModifiers();

			modifiers.ProjectileDamageMultiplier = modifiers.MeleeDamageMultiplier = 1.75f;
			modifiers.ProjectileKnockbackMultiplier = modifiers.MeleeKnockbackMultiplier = 1.5f;
			modifiers.ProjectileSpeedMultiplier = 2f;

			c.StatModifiers.Single = modifiers;
		});

		if (!Main.dedServ) {
			item.EnableComponent<ItemPowerAttackScreenShake>(c => {
				c.ScreenShake = new ScreenShake(0.5f, 0.5f);
			});

			item.EnableComponent<ItemPowerAttackSounds>(c => {
				c.Sound = ChargeSound;
				c.CancelPlaybackOnEnd = true;
			});
		}
	}

	public bool ShowItemCrosshair(Item item, Player player)
		=> true;
}
