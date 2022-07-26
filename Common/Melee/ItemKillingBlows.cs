using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemKillingBlows : ItemComponent
{
	private delegate void NPCDamageModifier(NPC npc, ref double damage);

	public static readonly float KillingBlowDamageMultiplier = 1.5f;
	public static readonly SoundStyle KillingBlowSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/KillingBlow", 2) {
		Volume = 0.6f,
		PitchVariance = 0.1f,
	};

	// Super questionable shenanigans are this feature's MP synchronization.
	private static readonly List<int> netNpcsWithPendingKillingBlows = new();

	[ThreadStatic]
	private static Player? playerCurrentlySwingingWeapons;

	public override void Load()
	{
		// Stores context.
		On.Terraria.Player.ItemCheck_MeleeHitNPCs += (orig, player, sItem, itemRectangle, originalDamage, knockback) => {
			playerCurrentlySwingingWeapons = player;

			try {
				orig(player, sItem, itemRectangle, originalDamage, knockback);
			}
			finally {
				playerCurrentlySwingingWeapons = null;
			}
		};

		// This IL edit implements killing blows. They have to run after all damage modifications, as it needs to check the final damage.
		IL.Terraria.NPC.StrikeNPC += context => {
			var cursor = new ILCursor(context);

			int damageLocalId = 0;

			cursor.GotoNext(
				MoveType.After,
				//	num *= (double)takenDamageMultiplier;
				i => i.MatchLdloc(out damageLocalId),
				i => i.Match(OpCodes.Ldarg_0),
				i => i.MatchLdfld(typeof(NPC), nameof(NPC.takenDamageMultiplier)),
				i => i.Match(OpCodes.Conv_R8),
				i => i.Match(OpCodes.Mul),
				//	{
				i => i.MatchStloc(out _)
			);

			cursor.HijackIncomingLabels(); // Make jumps be to before the upcoming insertion instead of after it.

			cursor.Emit(OpCodes.Ldarg_0);
			cursor.Emit(OpCodes.Ldloca, damageLocalId);
			cursor.EmitDelegate<NPCDamageModifier>(CheckForKillingBlow);
		};
	}

	internal static void EnqueueNPCForKillingBlowHit(int npcId)
	{
		netNpcsWithPendingKillingBlows.Add(npcId);
	}

	private static void CheckForKillingBlow(NPC npc, ref double damage)
	{
		bool sync = false;

		if (Main.netMode == NetmodeID.SinglePlayer || !netNpcsWithPendingKillingBlows.Remove(npc.whoAmI)) {
			if (playerCurrentlySwingingWeapons is not Player { HeldItem: Item item } player) {
				return;
			}

			if (!item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) || !item.TryGetGlobalItem(out ItemKillingBlows killingBlows)) {
				return;
			}

			if (!powerAttacks.Enabled || !killingBlows.Enabled || !powerAttacks.PowerAttack) {
				return;
			}

			sync = true;
		}

		if (DoKillingBlow(npc, ref damage) && sync) {
			MultiplayerSystem.SendPacket(new KillingBlowPacket(Main.LocalPlayer, npc));
		}
	}

	private static bool DoKillingBlow(NPC npc, ref double damage)
	{
		if (damage <= 0.0 || npc.life <= 0) {
			return false;
		}
		
		double multipliedDamage = damage * KillingBlowDamageMultiplier;

		if (npc.life - multipliedDamage <= 0.0d) {
			damage = multipliedDamage;

			if (!Main.dedServ) {
				SoundEngine.PlaySound(KillingBlowSound, npc.Center);
				CombatText.NewText(npc.getRect(), Color.MediumVioletRed, "Killing Blow!", true);
			}

			return true;
		}

		return false;
	}
}
