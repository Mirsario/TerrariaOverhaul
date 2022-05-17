using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ModEntities.Players;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Charging
{
	public sealed class ItemPowerAttacks : ItemComponent, IModifyCommonStatMultipliers, ICanDoMeleeDamage
	{
		public delegate bool CanStartPowerAttackDelegate(Item item, Player player);

		public float ChargeLengthMultiplier = 2f;
		public CommonStatMultipliers CommonStatMultipliers = CommonStatMultipliers.Default;

		public bool PowerAttack { get; private set; }

		public event Action<Item, Player>? OnStart;
		public event Action<Item, Player, float>? OnChargeStart;
		public event Action<Item, Player, float, float>? OnChargeUpdate;
		public event Action<Item, Player, float, float>? OnChargeEnd;
		public event CanStartPowerAttackDelegate? CanStartPowerAttack;

		public override void Load()
		{
			// AltFunctionUse hook doesn't fit, since it relies on 'ItemID.Sets.ItemsThatAllowRepeatedRightClick' for repeated uses.
			// Also it's better to execute power attack code after all other mods are done with their AltFunctionUse hooks.
			IL.Terraria.Player.ItemCheck_ManageRightClickFeatures += context => {
				var il = new ILCursor(context);

				int isButtonHeldLocalId = -1;

				il.GotoNext(
					// bool flag2 = flag;
					i => i.MatchLdcI4(0),
					i => i.MatchStloc(out isButtonHeldLocalId),
					i => i.MatchLdloc(isButtonHeldLocalId),
					i => i.MatchStloc(out _),
					// if (!ItemID.Sets.ItemsThatAllowRepeatedRightClick[inventory[selectedItem].type] && !Main.mouseRightRelease)
					i => i.MatchLdsfld(typeof(ItemID.Sets), nameof(ItemID.Sets.ItemsThatAllowRepeatedRightClick))
					// ...
				);

				il.GotoNext(
					MoveType.Before,
					// if (!controlUseItem && altFunctionUse == 1)
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(Player), nameof(Player.controlUseItem)),
					i => i.MatchBrtrue(out _),
					//
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(Player), nameof(Player.altFunctionUse)),
					i => i.MatchLdcI4(1)
					// ...
				);

				il.HijackIncomingLabels();

				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloc, isButtonHeldLocalId);
				il.EmitDelegate((Player player, bool isButtonHeld) => {
					if (!isButtonHeld || player.altFunctionUse != 0) {
						return;
					}

					if (player.HeldItem is not Item item) {
						return;
					}

					if (item.TryGetGlobalItem(out ItemPowerAttacks itemPowerAttacks) != true) {
						return;
					}

					if (itemPowerAttacks.AttemptPowerAttackStart(item, player)) {
						player.altFunctionUse = 1;
						player.controlUseItem = true;
					}
				});
			};
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

		public bool AttemptPowerAttackStart(Item item, Player player)
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
