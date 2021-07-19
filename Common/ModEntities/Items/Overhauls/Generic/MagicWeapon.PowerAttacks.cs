using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Utilities;
using TerrariaOverhaul.Common.ModEntities.Players;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	partial class MagicWeapon
	{
		public static float ChargeDamageScale => 2.5f;
		public static float ChargeKnockbackScale => 1.5f;
		public static float ChargeShootSpeedScale => 2f;
		public static float ChargeLengthScale => 2f;

		public static readonly SoundStyle ChargeSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicCharge", volume: 0.5f, pitchVariance: 0.1f);

		private SlotId chargeSoundInstance;

		public bool ChargedAttack { get; private set; }

		public override bool AltFunctionUse(Item item, Player player)
		{
			var itemCharging = item.GetGlobalItem<ItemCharging>();

			if(itemCharging.IsCharging) {
				return false;
			}

			if(!player.CheckMana(item)) {
				return false;
			}

			int chargeLength = CombinedHooks.TotalAnimationTime(item.useAnimation * ChargeLengthScale, player, item);

			if(!Main.dedServ) {
				chargeSoundInstance = SoundEngine.PlayTrackedSound(ChargeSound, player.Center);

				ScreenShakeSystem.New(
					new Gradient<float>(
						(0.0f, 0.0f),
						(0.5f, 0.1f),
						(1.0f, 15.0f)
					),
					chargeLength / (float)TimeSystem.LogicFramerate
				);
			}

			itemCharging.StartCharge(
				chargeLength,
				//Update
				(i, p, progress) => {
					p.itemTime = 2;
					p.itemAnimation = p.itemAnimationMax;
				},
				//End
				(i, p, progress) => {
					p.GetModPlayer<PlayerItemUse>().ForceItemUse();

					var magicWeapon = i.GetGlobalItem<MagicWeapon>();

					magicWeapon.ChargedAttack = true;

					magicWeapon.StopChargeSound();

					if(!Main.dedServ) {
						SoundEngine.PlaySound(MagicPowerfulBlastSound, player.Center);
					}
				},
				//Allow turning
				true
			);

			return false;
		}

		private void HoldItemCharging(Item item, Player player)
		{
			var itemCharging = item.GetGlobalItem<ItemCharging>();

			if(!Main.dedServ && chargeSoundInstance.IsValid) {
				var activeSound = SoundEngine.GetActiveSound(chargeSoundInstance);

				if(activeSound != null) {
					activeSound.Position = player.Center;
				} else {
					chargeSoundInstance = default;
				}
			}

			if(player.itemAnimation <= 0 && !itemCharging.IsCharging) {
				ChargedAttack = false;
			}

			base.HoldItem(item, player);
		}
		private void ShootCharging(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if(ChargedAttack) {
				damage = (int)(damage * ChargeDamageScale);
				knockback *= ChargeKnockbackScale;
				velocity *= ChargeShootSpeedScale;
			}
		}
		private void StopChargeSound()
		{
			if(!Main.dedServ && chargeSoundInstance.IsValid) {
				SoundEngine.GetActiveSound(chargeSoundInstance)?.Stop();
			}
		}
	}
}
