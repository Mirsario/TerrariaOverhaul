using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntitySources;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops;

public sealed class NPCHealthDrops : GlobalNPC
{
	public static readonly float DefaultHealthDropRange = ResourceDropUtils.DefaultResourceDropRange;
	public static readonly int HealthDropItemType = ItemID.Heart;

	public override void Load()
	{
		On.Terraria.NPC.NPCLoot += (orig, npc) => {
			orig(npc);

			if (Main.netMode == NetmodeID.MultiplayerClient) {
				return;
			}

			if (npc.damage <= 0 || NPCID.Sets.ProjectileNPC[npc.type]) {
				return;
			}

			DropHealthFromKill(npc);
		};

		On.Terraria.NPC.NPCLoot_DropHeals += (orig, npc, closestPlayer) => {
			// Juuuust don't.
		};
	}

	public static void DropHealthFromKill(NPC npc)
	{
		int maxAmountToDrop = 0;
		var dropPosition = npc.Center;
		var dropsByPlayer = new Dictionary<Player, int>();

		foreach (var player in ActiveEntities.Players) {
			int dropCount = CalculateCommonHealthDropAmount(player, dropPosition);

			if (dropCount > 0) {
				dropsByPlayer[player] = dropCount;
				maxAmountToDrop = Math.Max(maxAmountToDrop, dropCount);
			}
		}

		DropHealth(npc, maxAmountToDrop, dropsByPlayer);
	}
	
	public static int CalculateCommonHealthDropAmount(Player player, Vector2 dropPosition)
	{
		float dropRange = DefaultHealthDropRange; // Used both in culling players and checking for existing drops

		// Ignore players that are too far away from the drop position
		if (!player.WithinRange(dropPosition, dropRange)) {
			return 0;
		}

		// Initial - How many resource drops the player needs
		int dropCount = ResourceDropUtils.GetResourceDropsNeededByPlayer(player.statLife, player.statLifeMax2, HealthPickupChanges.HealthPerPickup);

		// Cull by the amount of drops nearby to the player
		int existingDrops = ResourceDropUtils.CountItemsOfTypeWithinRange(HealthDropItemType, dropPosition, dropRange);

		dropCount = Math.Max(0, dropCount - existingDrops);

		// Cull the drop count by the player's current health ratio
		dropCount = Math.Min(dropCount, GetMaxHealthDropsForPlayersHealthRatio(player));

		return dropCount;
	}

	public static int GetMaxHealthDropsForPlayersHealthRatio(Player player)
	{
		float healthFactor = player.statLife / (float)player.statLifeMax2;

		return healthFactor switch {
			<= 1f / 4f => 3,
			<= 1f / 3f => 2,
			_ => 1,
		};
	}

	public static void DropHealth(NPC npc, int maxAmount, Dictionary<Player, int>? perPlayerAmount = null)
	{
		// Amount of time to occupy the slot for. The extra '60' doesn't affect actual behavior of the pickup.
		int maxExpectedLifeTime = HealthPickupChanges.MaxLifeTime + 60;
		var entitySource = new EntitySource_EntityResourceDrops(npc);

		ResourceDropUtils.DropResource(entitySource, npc.Center, HealthDropItemType, maxAmount, maxExpectedLifeTime, perPlayerAmount);
	}
}
