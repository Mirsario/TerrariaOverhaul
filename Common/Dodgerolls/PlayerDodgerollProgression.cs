using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Dodgerolls;

public sealed class ItemDodgerollProgression : GlobalItem
{
	public enum RecoveryType
	{
		Undefined,
		Jump,
	}

	public static Dictionary<int, float>[] BoostingItems { get; set; } = Array.Empty<Dictionary<int, float>>();
	public static (int delta, int[] items)[] ChargeItems { get; set; } = Array.Empty<(int, int[])>();
	public static (int delta, Func<Player, bool> getter)[] ChargeEffects { get; set; } = Array.Empty<(int, Func<Player, bool>)>();
	public static (int delta, RecoveryType type, int identifier, int[] items)[] ActionRecovery { get; set; } = Array.Empty<(int, RecoveryType, int, int[])>();

	public override void SetStaticDefaults()
	{
		// Effects that increase or decrease the max charge amount.
		ChargeEffects = new (int delta, Func<Player, bool> getter)[] {
			// + All wings reduce it by 1
			(-1, p => p.wingsLogic != 0),
		};

		// Items that increase or decrease the max charge amount.
		ChargeItems = new[] {
			(+1, new int[] { ItemID.HermesBoots, ItemID.FlurryBoots, ItemID.SailfishBoots, ItemID.SandBoots, ItemID.SpectreBoots, ItemID.LightningBoots, ItemID.FairyBoots, ItemID.HellfireTreads, ItemID.FrostsparkBoots, }),
			(+1, new int[] { ItemID.MasterNinjaGear, ItemID.Tabi, }),
			(+1, new int[] { ItemID.EoCShield }),
			(+1, new int[] { ItemID.Aglet, ItemID.AnkletoftheWind }),
		};

		// These speed up recovery time
		BoostingItems = new Dictionary<int, float>[] {
			new() {
				{ ItemID.CopperWatch,   1.05f },
				{ ItemID.TinWatch,      1.05f },
				{ ItemID.SilverWatch,   1.15f },
				{ ItemID.TungstenWatch, 1.15f },
				{ ItemID.GoldWatch,     1.25f },
				{ ItemID.PlatinumWatch, 1.25f },
				{ ItemID.Stopwatch,     1.30f },
			}
		};

		// These recover an entire charge when used
		//ActionRecovery = new[] {
		//	(+1, RecoveryType.Jump, ModContent.GetInstance<CloudInABottleJump>().Type, new int[] {
		//		ItemID.CloudinaBottle, ItemID.BundleofBalloons, ItemID.HorseshoeBundle, ItemID.BlueHorseshoeBalloon, ItemID.CloudinaBalloon,
		//	}),
		//	(+1, RecoveryType.Jump, ModContent.GetInstance<BlizzardInABottleJump>().Type, new int[] {
		//		ItemID.BlizzardinaBottle, ItemID.BlizzardinaBalloon, ItemID.BundleofBalloons, ItemID.HorseshoeBundle, ItemID.WhiteHorseshoeBalloon,
		//	}),
		//	(+1, RecoveryType.Jump, ModContent.GetInstance<SandstormInABottleJump>().Type, new int[] {
		//		ItemID.SandstorminaBottle, ItemID.SandstorminaBalloon, ItemID.BundleofBalloons, ItemID.HorseshoeBundle, ItemID.YellowHorseshoeBalloon,
		//	}),
		//};
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		Color color = Color.Lerp(Color.MediumVioletRed, Main.DiscoColor, 0.15f);

		// Charge

		int chargeDelta = ChargeItems.Length != 0
			? ChargeItems.Max(t => t.items.Contains(item.type) ? t.delta : 0)
			: 0;

		if (chargeDelta != 0) {
			tooltips.Add(new(Mod, "TerrariaOverhaul/DodgerollCharge", $"◙ +{chargeDelta} Dodgeroll charges") {
				OverrideColor = color
			});
		}

		// Recovery speed

		float chargeTimeMultiplier = BoostingItems.Length != 0
			? BoostingItems.Max(d => d.TryGetValue(item.type, out float v) ? v : 1.0f)
			: 1f;

		if (chargeTimeMultiplier != 1f) {
			int percent = (int)MathF.Ceiling((chargeTimeMultiplier - 1.0f) * 100);
			char sign = percent > 0 ? '+' : '-';

			tooltips.Add(new(Mod, "TerrariaOverhaul/DodgerollBoost", $"◙ {sign}{percent}% Dodgeroll recovery speed") {
				OverrideColor = color
			});
		}

		// Recovery on use

		bool recoversCharges = ActionRecovery.Any(t => t.items.Contains(item.type));

		if (recoversCharges) {
			tooltips.Add(new(Mod, "TerrariaOverhaul/DodgerollRecovery", $"◙ Recharges dodgerolls on use") {
				OverrideColor = color
			});
		}
	}
}

public sealed class PlayerDodgerollProgression : ModPlayer
{
	private bool[] wasJumpActive = Array.Empty<bool>();

	public override void UpdateEquips()
	{
		Player.TryGetModPlayer(out PlayerDodgerolls dodgerolls);

		Charges(dodgerolls);
		Cooldowns(dodgerolls);
		Recovery(dodgerolls);
	}

	private void Charges(PlayerDodgerolls dodgerolls)
	{
		void Modify(int number)
			=> dodgerolls.Stats.MaxCharges = (uint)Math.Max(0, (int)(dodgerolls.Stats.MaxCharges + number));

		foreach (var tuple in ItemDodgerollProgression.ChargeItems) {
			if (Player.HasAccessory(any: true, tuple.items)) {
				Modify(tuple.delta);
			}
		}
		foreach (var tuple in ItemDodgerollProgression.ChargeEffects) {
			if (tuple.getter(Player)) {
				Modify(tuple.delta);
			}
		}
	}

	private void Cooldowns(PlayerDodgerolls dodgerolls)
	{
		float totalMultiplier = 1.0f;
		var boostingItems = ItemDodgerollProgression.BoostingItems;

		for (int i = 0; i < boostingItems.Length; i++) {
			totalMultiplier *= boostingItems[i].Max(p => Player.HasAccessory(p.Key) ? p.Value : 1f);
		}

		if (totalMultiplier != 1.0f) {
			dodgerolls.Stats.CooldownLength = (uint)(dodgerolls.Stats.CooldownLength / totalMultiplier);
		}
	}

	private void Recovery(PlayerDodgerolls dodgerolls)
	{
		var recoveryTypes = ItemDodgerollProgression.ActionRecovery;
		var extraJumps = Player.ExtraJumps;

		Array.Resize(ref wasJumpActive, extraJumps.Length);

		for (int i = 0; i < recoveryTypes.Length; i++) {
			ref readonly var tuple = ref recoveryTypes[i];

			switch (tuple.type) {
				case ItemDodgerollProgression.RecoveryType.Jump:
					ref var jumpState = ref extraJumps[tuple.identifier];

					if (jumpState.Active && !wasJumpActive[tuple.identifier]) {
						dodgerolls.TirednessTimer.Offset(-60);
					}

					break;
			}
		}

		for (int i = 0; i < extraJumps.Length; i++) {
			wasJumpActive[i] = extraJumps[i].Active;
		}
	}
}
