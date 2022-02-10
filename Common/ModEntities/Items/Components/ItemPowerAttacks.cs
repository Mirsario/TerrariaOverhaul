using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ModEntities.Players;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
	public sealed class ItemPowerAttacks : ItemComponent, IModifyCommonStatMultipliers, ICanDoMeleeDamage
	{
		public delegate bool CanStartPowerAttackDelegate(Item item, Player player);

		public float ChargeLengthMultiplier = 2f;
		public CommonStatMultipliers CommonStatMultipliers = CommonStatMultipliers.Default;

		public bool PowerAttack { get; private set; }

		public event Action<Item, Player> OnStart;
		public event Action<Item, Player, float> OnChargeStart;
		public event Action<Item, Player, float, float> OnChargeUpdate;
		public event Action<Item, Player, float, float> OnChargeEnd;
		public event CanStartPowerAttackDelegate CanStartPowerAttack;

		public override bool AltFunctionUse(Item item, Player player)
		{
			if (!Enabled) {
				return false;
			}

			var itemCharging = item.GetGlobalItem<ItemCharging>();

			if (itemCharging.IsCharging || player.itemAnimation > 0) {
				return false;
			}

			if (!player.CheckMana(item)) {
				return false;
			}

			if (CanStartPowerAttack != null && CanStartPowerAttack.GetInvocationList().Any(func => !((CanStartPowerAttackDelegate)func)(item, player))) {
				return false;
			}

			int chargeLength = CombinedHooks.TotalAnimationTime(item.useAnimation * ChargeLengthMultiplier, player, item);

			OnChargeStart?.Invoke(item, player, chargeLength);

			itemCharging.StartCharge(
				chargeLength,
				// Update
				(i, p, progress) => {
					p.itemTime = 2;
					p.itemAnimation = p.itemAnimationMax;

					OnChargeUpdate?.Invoke(i, p, chargeLength, progress);
				},
				// End
				(i, p, progress) => {
					i.GetGlobalItem<ItemPowerAttacks>().PowerAttack = true;

					p.GetModPlayer<PlayerItemUse>().ForceItemUse();

					OnChargeEnd?.Invoke(i, p, chargeLength, progress);
				},
				// Allow turning
				true
			);

			return false;
		}

		public override void UseAnimation(Item item, Player player)
		{
			if (PowerAttack) {
				OnStart?.Invoke(item, player);
			}
		}

		public override void HoldItem(Item item, Player player)
		{
			if (player.itemAnimation <= 1 && !item.GetGlobalItem<ItemCharging>().IsCharging) {
				PowerAttack = false;
			}
		}

		public void ModifyCommonStatMultipliers(Item item, Player player, ref CommonStatMultipliers multipliers)
		{
			if (PowerAttack) {
				multipliers *= CommonStatMultipliers;
			}
		}

		bool ICanDoMeleeDamage.CanDoMeleeDamage(Item item, Player player)
			=> !item.TryGetGlobalItem<ItemCharging>(out var itemCharging) || !itemCharging.IsCharging;
	}
}
