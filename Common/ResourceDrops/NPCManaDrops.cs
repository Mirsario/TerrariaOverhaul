using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntitySources;
using TerrariaOverhaul.Content.Dusts;
using TerrariaOverhaul.Utilities;

#pragma warning disable CA1822 // Mark members as static

namespace TerrariaOverhaul.Common.ResourceDrops;

public sealed class NPCManaDrops : GlobalNPC
{
	public static readonly float DefaultManaDropRange = ResourceDropUtils.DefaultResourceDropRange;
	public static readonly int ManaDropItemType = ItemID.Star;
	public static readonly int ManaDropCooldownTime = 15;

	private int manaDropCooldown = ManaDropCooldownTime;
	private int manaPickupsToDropInTotal;
	private int numTimesManaPickupDropsWereAttempted;
	private bool hasSegments;
	
	public override bool InstancePerEntity => true;

	public override void SetDefaults(NPC npc)
	{
		if (npc.damage <= 0 || NPCID.Sets.ProjectileNPC[npc.type]) {
			return;
		}

		float totalManaToDrop = 15f;

		if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) {
			totalManaToDrop = npc.lifeMax / 6f; // This is so not going to be balanced...
		}

		manaPickupsToDropInTotal = (int)MathF.Ceiling(totalManaToDrop / ManaPickupChanges.ManaPerPickup);
	}

	public override void PostAI(NPC npc)
	{
		var mainNpc = npc.GetMainSegment();

		if (mainNpc != npc) {
			if (mainNpc.TryGetGlobalNPC(out NPCManaDrops npcManaDrops)) {
				npcManaDrops.hasSegments = true;
			}

			return;
		}

		int expectedPickupAmount = CalculateExpectedDroppedManaPickupAmount(npc);

		if (expectedPickupAmount <= numTimesManaPickupDropsWereAttempted) {
			return;
		}

		bool localPlayerNeedsMana = Main.netMode != NetmodeID.Server && CalculateManaDropAmountForPlayer(Main.LocalPlayer, npc.Center, expectedPickupAmount) > 0;

		if (--manaDropCooldown <= 0) {
			bool resetCooldown = false;

			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (DropAccumulatedMana(npc, expectedPickupAmount, maxDrops: 1)) {
					resetCooldown = true;
					numTimesManaPickupDropsWereAttempted++;
				}
			} else if (localPlayerNeedsMana) {
				// Do nothing, just assume that the server dropped pickups for us, since we kinda need them.
				// There is likely to be some desync, but not that much...
				resetCooldown = true;
				numTimesManaPickupDropsWereAttempted++;
			}

			if (resetCooldown) {
				manaDropCooldown = ManaDropCooldownTime;
			}
		}

		if (!Main.dedServ && localPlayerNeedsMana) {
			var glowingNpc = hasSegments ? npc.GetRandomSegment() : npc;
			float lightPulse = (float)Math.Sin(Main.GameUpdateCount / 60f * 10f) * 0.5f + 0.5f;

			Lighting.AddLight(glowingNpc.Center, Color.Lerp(Color.BlueViolet, Color.LightSkyBlue, lightPulse).ToVector3());

			if (Main.GameUpdateCount % 2 == 0) {
				Vector2 point = glowingNpc.getRect().GetRandomPoint();

				Dust.NewDustPerfect(point, ModContent.DustType<ManaDust>(), Vector2.Zero);
			}
		}
	}

	public override void OnKill(NPC npc)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient) {
			DropAccumulatedMana(npc);
		}
	}

	public int CalculateExpectedDroppedManaPickupAmount(NPC npc)
	{
		if (npc.life >= npc.lifeMax) {
			return 0;
		}
		
		float healthFactor = Math.Max(npc.life, 0) / (float)Math.Max(npc.lifeMax, 1);
		int result = (int)MathF.Floor((1f - healthFactor) * manaPickupsToDropInTotal);

		return result;
	}

	public int CalculateManaDropAmountForPlayer(Player player, Vector2 dropPosition, int currentExpectedAmount)
	{
		if (currentExpectedAmount <= 0) {
			return 0;
		}

		float dropRange = DefaultManaDropRange; // Used both in culling players and checking for existing drops

		int result = currentExpectedAmount;

		/*
		// Cull based on how many resource drops were already given to this player
		if (manaPickupsDroppedPerPlayer?.TryGetValue(player.whoAmI, out int manaPickupsDropped) == true) {
			result -= manaPickupsDropped;

			if (result <= 0) {
				return 0;
			}
		}
		*/

		// Ignore players that are too far away from the drop position
		if (!player.WithinRange(dropPosition, dropRange)) {
			return 0;
		}

		// Cull by how many resource drops the player needs
		int neededDrops = ResourceDropUtils.GetResourceDropsNeededByPlayer(player.statMana, player.statManaMax2, ManaPickupChanges.ManaPerPickup);

		result = Math.Min(result, neededDrops);

		/*
		if (result <= 0) {
			return 0;
		}

		// Cull by the amount of drops nearby to the player
		int existingDrops = ResourceDropUtils.CountItemsOfTypeWithinRange(ManaDropItemType, dropPosition, dropRange);

		result = Math.Max(0, result - existingDrops);
		*/

		return result;
	}

	public bool DropAccumulatedMana(NPC npc, int? currentExpectedAmount = null, int? maxDrops = null)
	{
		var mainNpc = npc.GetMainSegment();

		if (npc != mainNpc) {
			return false;
		}

		currentExpectedAmount ??= CalculateExpectedDroppedManaPickupAmount(npc);

		if (currentExpectedAmount.Value <= 0) {
			return false;
		}

		maxDrops = Math.Min(maxDrops ?? int.MaxValue, currentExpectedAmount.Value - numTimesManaPickupDropsWereAttempted);

		if (maxDrops.Value <= 0) {
			return false;
		}

		int maxAmountDropped = 0;
		var droppingNpc = hasSegments ? npc.GetRandomSegment() : npc;
		var dropPosition = droppingNpc.Center;
		var dropsByPlayer = new Dictionary<Player, int>();
		
		foreach (var player in ActiveEntities.Players) {
			int personalDropCount = Math.Min(CalculateManaDropAmountForPlayer(player, dropPosition, currentExpectedAmount.Value), maxDrops.Value);

			if (personalDropCount > 0) {
				dropsByPlayer[player] = personalDropCount;
				maxAmountDropped = Math.Max(maxAmountDropped, personalDropCount);
			}
		}

		DropMana(droppingNpc, maxAmountDropped, dropsByPlayer);

		return true;
	}

	public static void DropMana(NPC npc, int maxAmount, Dictionary<Player, int>? perPlayerAmount = null)
	{
		var entitySource = new EntitySource_EntityResourceDrops(npc);

		ResourceDropUtils.DropResource(entitySource, npc.Center, ManaDropItemType, maxAmount, ManaPickupChanges.MaxLifeTime + 60, perPlayerAmount);
	}
}
