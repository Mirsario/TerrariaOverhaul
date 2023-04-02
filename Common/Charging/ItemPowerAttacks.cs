using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Charging;

public sealed class ItemPowerAttacks : ItemComponent, IModifyCommonStatMultipliers, ICanDoMeleeDamage
{
	public delegate bool CanStartPowerAttackDelegate(Item item, Player player);

	public float ChargeLengthMultiplier = 2f;
	public CommonStatMultipliers CommonStatMultipliers = CommonStatMultipliers.Default;

	private Timer charge;

	public bool PowerAttack { get; private set; }

	public Timer Charge => charge;
	public bool IsCharging => Charge.Active;

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

			int initialCheckSuccessLocalId = context.Body.Variables.Count;
			
			il.Body.Variables.Add(new VariableDefinition(context.Import(typeof(bool))));

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldloc, isButtonHeldLocalId);
			il.EmitDelegate((Player player, bool isButtonHeld) => {
				return isButtonHeld && player.altFunctionUse == 0;
			});
			il.Emit(OpCodes.Stloc, initialCheckSuccessLocalId);

			// Move to right before the end of the method
			il.GotoNext(MoveType.Before, i => i.Match(OpCodes.Ret));
			il.HijackIncomingLabels();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldloc, initialCheckSuccessLocalId);
			il.EmitDelegate((Player player, bool initialCheckSuccess) => {
				if (!initialCheckSuccess || player.altFunctionUse != 0 || player.shieldRaised) {
					return;
				}

				if (player.HeldItem is not Item item) {
					return;
				}

				if (item.TryGetGlobalItem(out ItemPowerAttacks itemPowerAttacks) != true) {
					return;
				}

				itemPowerAttacks.AttemptPowerAttackStart(item, player);

				/*
				player.altFunctionUse = 1;
				player.controlUseItem = true;
				*/
			});
		};
	}

	public override void HoldItem(Item item, Player player)
	{
		if (IsCharging) {
			// The charge is ongoing
			ChargeUpdate(item, player);
		} else {
			// The charge just ended
			if (Charge.UnclampedValue == 0) {
				ChargeEnd(item, player);
			}

			if (player.itemAnimation <= 1) {
				PowerAttack = false;
			}
		}
	}

	public bool AttemptPowerAttackStart(Item item, Player player)
	{
		if (!Enabled || !player.IsLocal()) {
			return false;
		}

		if (player.itemAnimation > 0 || IsCharging) {
			return false;
		}

		if (!ItemLoader.CanUseItem(item, player)) {
			return false;
		}

		if (!player.CheckMana(item)) {
			return false;
		}

		if (!ICanStartPowerAttack.Invoke(item, player)) {
			return false;
		}
		
		uint chargeLength = (uint)CombinedHooks.TotalAnimationTime(item.useAnimation * ChargeLengthMultiplier, player, item);

		StartPowerAttack(item, player, chargeLength);

		return true;
	}

	public void StartPowerAttack(Item item, Player player, uint chargeLength)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient && player.IsLocal()) {
			MultiplayerSystem.SendPacket(new PowerAttackStartPacket(player, (int)chargeLength));
		}

		charge.Set(chargeLength);
	}

	public void ModifyCommonStatMultipliers(Item item, Player player, ref CommonStatMultipliers multipliers)
	{
		if (PowerAttack) {
			multipliers *= CommonStatMultipliers;
		}
	}

	private void ChargeUpdate(Item item, Player player)
	{
		player.itemTime = 2;
		player.itemAnimation = player.itemAnimationMax;
	}

	private void ChargeEnd(Item item, Player player)
	{
		item.GetGlobalItem<ItemPowerAttacks>().PowerAttack = true;
		player.GetModPlayer<PlayerItemUse>().ForceItemUse();
	}

	bool ICanDoMeleeDamage.CanDoMeleeDamage(Item item, Player player)
		=> !IsCharging;
}
