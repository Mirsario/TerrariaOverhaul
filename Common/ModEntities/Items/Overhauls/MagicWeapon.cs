using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Camera.ScreenShakes;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public partial class MagicWeapon : ItemOverhaul, IShowItemCrosshair
	{
		public static readonly ModSoundStyle MagicBlastSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicBlast", 3, pitchVariance: 0.1f);
		public static readonly ModSoundStyle MagicPowerfulBlastSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicPowerfulBlast", pitchVariance: 0.4f);
		public static readonly ModSoundStyle ChargeSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicCharge", volume: 0.5f, pitchVariance: 0.1f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			// Ignore weapons with non-magic damage types
			if (item.DamageType != DamageClass.Magic && !item.DamageType.CountsAs(DamageClass.Magic)) {
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

			item.EnableComponent<ItemPowerAttacks>(c => {
				c.ChargeLengthMultiplier = 2f;
				c.CommonStatMultipliers.ProjectileDamageMultiplier = 1.75f;
				c.CommonStatMultipliers.ProjectileKnockbackMultiplier = 1.5f;
				c.CommonStatMultipliers.ProjectileSpeedMultiplier = 2f;

				c.OnChargeStart += (item, player, chargeLength) => {
					if (Main.dedServ) {
						return;
					}

					ScreenShakeSystem.New(
						new Gradient<float>(
							(0.0f, 0.0f),
							(0.5f, 0.1f),
							(1.0f, 15.0f)
						),
						chargeLength / TimeSystem.LogicFramerate
					);
				};
			});

			if (!Main.dedServ) {
				item.EnableComponent<ItemPowerAttackSounds>(c => {
					c.Sound = ChargeSound;
					c.CancelPlaybackOnEnd = true;
				});

				item.EnableComponent<ItemUseVisualRecoil>(c => {
					c.Power = 10f;
				});

				item.EnableComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(4f, 0.2f);
				});
			}
		}

		public bool ShowItemCrosshair(Item item, Player player) => true;
	}
}
