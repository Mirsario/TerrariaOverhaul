using System;
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

	public static readonly SoundStyle KillingBlowSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/KillingBlow", 2) {
		Volume = 0.6f,
		PitchVariance = 0.1f,
	};

	[ThreadStatic]
	private static Player? playerCurrentlySwingingWeapons;

	private int killingBlowCount;

	public int MaxKillingBlowsPerUse { get; set; } = 1;
	public float DamageMultiplier { get; set; } = 1.5f;

	public override void Load()
	{
		// Stores context.
		On_Player.ItemCheck_MeleeHitNPCs += (orig, player, sItem, itemRectangle, originalDamage, knockback) => {
			playerCurrentlySwingingWeapons = player;

			try {
				orig(player, sItem, itemRectangle, originalDamage, knockback);
			}
			finally {
				playerCurrentlySwingingWeapons = null;
			}
		};

		// This IL edit implements killing blows. They have to run after all damage modifications, as it needs to check the final damage.
		IL_NPC.StrikeNPC += context => {
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

	public override void HoldStyle(Item item, Player player, Rectangle heldItemFrame)
	{
		if (Enabled && !player.ItemAnimationActive) {
			killingBlowCount = 0;
		}
	}

	private void KillingBlow(NPC npc, ref double damage)
	{
		if (killingBlowCount >= MaxKillingBlowsPerUse) {
			return;
		}

		if (damage <= 0.0 || npc.life <= 0) {
			return;
		}

		if (NPCID.Sets.ProjectileNPC[npc.type]) {
			return;
		}

		double multipliedDamage = damage * DamageMultiplier;

		if (npc.life - multipliedDamage <= 0.0d) {
			damage = multipliedDamage;

			if (Main.netMode != NetmodeID.Server) {
				var effectsPosition = (Vector2Int)npc.Center;

				CreateEffects(effectsPosition);

				if (Main.netMode == NetmodeID.MultiplayerClient) {
					MultiplayerSystem.SendPacket(new KillingBlowEffectsPacket(Main.LocalPlayer, effectsPosition));
				}
			}

			killingBlowCount++;
		}
	}

	internal static void CreateEffects(Vector2Int position)
	{
		if (!Main.dedServ) {
			SoundEngine.PlaySound(KillingBlowSound, position);
			CombatText.NewText(new Rectangle(position.X, position.Y, 0, 0), Color.MediumVioletRed, "Killing Blow!", true);
		}
	}

	private static void CheckForKillingBlow(NPC npc, ref double damage)
	{
		if (playerCurrentlySwingingWeapons is not Player { HeldItem: Item item } player || !player.IsLocal()) {
			return;
		}

		if (!item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) || !item.TryGetGlobalItem(out ItemKillingBlows killingBlows)) {
			return;
		}

		if (!powerAttacks.Enabled || !killingBlows.Enabled || !powerAttacks.PowerAttack) {
			return;
		}

		killingBlows.KillingBlow(npc, ref damage);
	}
}
