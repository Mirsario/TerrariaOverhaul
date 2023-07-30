using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Archery;

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

		item.EnableComponent<ItemPowerAttackHover>(c => {
			c.ActivationVelocityRange = new Vector4(
				// Minimum X & Y
				float.NegativeInfinity, float.NegativeInfinity,
				// Maximum X & Y
				float.PositiveInfinity, 3.0f
			);
			c.ControlsVelocityRecoil = true;
		});

		item.EnableComponent<ItemUseVelocityRecoil>(e => {
			e.BaseVelocity = new(15.0f, 15.0f);
			e.MaxVelocity = new(6.0f, 8.5f);
		}).SetEnabled(item, false);

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
				//new(0.750f, strongest),
				new(1.000f, strongest),
			});
		});

		item.EnableComponent<ItemPowerAttackScreenShake>(c => {
			Gradient<float> chargeScreenShakePowerGradient = new(
				(0.000f, 0.000f * 0.1f),
				(0.250f, 0.025f * 0.1f),
				(0.500f, 0.090f * 0.1f),
				(1.000f, 0.200f * 0.1f)
			);
			c.ScreenShake = new ScreenShake(chargeScreenShakePowerGradient, 0.5f);
		});

		item.EnableComponent<ItemPrimaryUseCharging>(c => {
			// One third of the vanilla use time is spent on the charge, two thirds remain.
			c.UseLengthMultiplier = 2f / 3f;
			c.ChargeLengthMultiplier = 1f / 3f;
		});

		if (!Main.dedServ) {
			item.EnableComponent<ItemPowerAttackSounds>(c => {
				c.Sound = ChargeSound;
				c.CancelPlaybackOnEnd = true;
			});
		}
	}
}
