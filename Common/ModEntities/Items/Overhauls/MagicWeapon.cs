using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ModEntities.Items.Shared;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public partial class MagicWeapon : AdvancedItem, IShowItemCrosshair
	{
		public static readonly ModSoundStyle MagicBlastSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicBlast", 3, pitchVariance: 0.1f);
		public static readonly ModSoundStyle MagicPowerfulBlastSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicPowerfulBlast", pitchVariance: 0.4f);
		public static readonly ModSoundStyle ChargeSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicCharge", volume: 0.5f, pitchVariance: 0.1f);

		public override ScreenShake OnUseScreenShake => new(4f, 0.2f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			//Ignore weapons with non-magic damage types
			if(item.DamageType != DamageClass.Magic && !item.DamageType.CountsAs(DamageClass.Magic)) {
				return false;
			}

			//Avoid tools and placeables
			if(item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
				return false;
			}

			//Ignore weapons that don't shoot, don't use mana, or deal hitbox damage 
			if(item.shoot <= ProjectileID.None || item.mana <= 0 || !item.noMelee) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			if(item.UseSound == SoundID.Item43) {
				item.UseSound = MagicBlastSound;
			}

			var powerAttacks = item.GetGlobalItem<PowerAttacks>();

			powerAttacks.Enabled = true;
			powerAttacks.ChargeLengthMultiplier = 2f;
			powerAttacks.CommonStatMultipliers.ProjectileDamageMultiplier = 2.5f;
			powerAttacks.CommonStatMultipliers.ProjectileKnockbackMultiplier = 1.5f;
			powerAttacks.CommonStatMultipliers.ProjectileSpeedMultiplier = 2f;

			powerAttacks.OnChargeStart += (item, player, chargeLength) => {
				if(Main.dedServ) {
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

			if(!Main.dedServ) {
				var powerAttackSounds = item.GetGlobalItem<PowerAttackSounds>();

				powerAttackSounds.Enabled = true;
				powerAttackSounds.Sound = ChargeSound;
			}
		}

		public bool ShowItemCrosshair(Item item, Player player) => true;
	}
}
