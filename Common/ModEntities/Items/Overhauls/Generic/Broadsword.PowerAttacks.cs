using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Utilities;
using TerrariaOverhaul.Common.ModEntities.Players;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic
{
	partial class Broadsword
	{
		public const float ChargeRangeScale = 1.33f;
		public const float ChargeDamageScale = 1.5f;
		public const float ChargeKnockbackScale = 1.5f;
		public const float ChargeLengthScale = 1.5f;

		public bool ChargedAttack { get; private set; }

		public override float GetAttackRange(Item item)
		{
			float range = base.GetAttackRange(item);

			if(ChargedAttack) {
				range *= ChargeRangeScale;
			}

			return range;
		}
		public override bool AltFunctionUse(Item item, Player player)
		{
			var itemCharging = item.GetGlobalItem<ItemCharging>();

			if(itemCharging.IsCharging) {
				return false;
			}

			int chargeLength = CombinedHooks.TotalAnimationTime(item.useAnimation * ChargeLengthScale, player, item);

			//These 2 lines only affect animations.
			FlippedAttack = false;
			AttackDirection = Vector2.UnitX * player.direction;

			itemCharging.StartCharge(
				chargeLength,
				//Update
				(i, p, progress) => {
					p.itemAnimation = p.itemAnimationMax;

					var broadsword = i.GetGlobalItem<Broadsword>();

					broadsword.AttackDirection = Vector2.Lerp(broadsword.AttackDirection, p.LookDirection(), 5f * TimeSystem.LogicDeltaTime);
				},
				//End
				(i, p, progress) => {
					p.GetModPlayer<PlayerItemUse>().ForceItemUse();
					i.GetGlobalItem<Broadsword>().ChargedAttack = true;
				},
				//Allow turning
				true
			);

			return false;
		}

		private void ModifyHitNPCCharging(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if(ChargedAttack) {
				damage = (int)(damage * ChargeDamageScale);
				knockback *= ChargeKnockbackScale;
			}
		}
		private void HoldItemCharging(Item item, Player player)
		{
			var itemCharging = item.GetGlobalItem<ItemCharging>();

			if(player.itemAnimation <= 0 && !itemCharging.IsCharging) {
				ChargedAttack = false;
			}

			base.HoldItem(item, player);
		}
	}
}
