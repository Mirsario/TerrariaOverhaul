using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntitySources;
using TerrariaOverhaul.Content.Dusts;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops
{
	public sealed class NPCManaDrops : GlobalNPC
	{
		public static readonly float DefaultManaDropRange = ResourceDropUtils.DefaultResourceDropRange;
		public static readonly int ManaDropItemType = ItemID.Star;
		public static readonly int ManaDropCooldownTime = 15;

		private int manaDropCooldown = ManaDropCooldownTime;
		private int manaPickupsToDropInTotal;
		private int manaPickupsDropped;

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
			int expectedPickupAmount = CalculateExpectedDroppedManaPickupAmount(npc);

			if (expectedPickupAmount <= manaPickupsDropped) {
				return;
			}

			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (--manaDropCooldown <= 0 && DropAccumulatedMana(npc, expectedPickupAmount)) {
					manaDropCooldown = ManaDropCooldownTime;
				}
			}

			if (!Main.dedServ && CanDropManaForPlayer(Main.LocalPlayer, npc.Center)) {
				float lightPulse = (float)Math.Sin(Main.GameUpdateCount / 60f * 10f) * 0.5f + 0.5f;

				Lighting.AddLight(npc.Center, Color.Lerp(Color.BlueViolet, Color.LightSkyBlue, lightPulse).ToVector3());

				if (Main.GameUpdateCount % 2 == 0) {
					Vector2 point = npc.getRect().GetRandomPoint();

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
			float healthFactor = Math.Max(npc.life, 0) / (float)Math.Max(npc.lifeMax, 1);
			int result = (int)MathF.Floor((1f - healthFactor) * manaPickupsToDropInTotal);

			return result;
		}

		public bool DropAccumulatedMana(NPC npc, int? currentExpectedAmount = null)
		{
			currentExpectedAmount ??= CalculateExpectedDroppedManaPickupAmount(npc);

			int newAmount = currentExpectedAmount.Value - manaPickupsDropped;

			if (newAmount <= 0) {
				return false;
			}

			var dropsByPlayer = new Dictionary<Player, int>();

			foreach (var player in ActiveEntities.Players) {
				if (CanDropManaForPlayer(player, npc.Center)) {
					dropsByPlayer[player] = newAmount;
				}
			}

			DropMana(npc, newAmount, dropsByPlayer);

			manaPickupsDropped = currentExpectedAmount.Value;

			return true;
		}
		
		public static bool CanDropManaForPlayer(Player player, Vector2 dropPosition)
		{
			float dropRange = DefaultManaDropRange;

			// Ignore players that are too far away from the drop position
			if (!player.WithinRange(dropPosition, dropRange)) {
				return false;
			}

			// Ignore players with full mana
			if (player.statMana >= player.statManaMax2) {
				return false;
			}

			return true;
		}

		public static void DropMana(NPC npc, int maxAmount, Dictionary<Player, int>? perPlayerAmount = null)
		{
			var entitySource = new EntitySource_EntityResourceDrops(npc);

			ResourceDropUtils.DropResource(entitySource, npc.Center, ManaDropItemType, maxAmount, ManaPickupChanges.MaxLifeTime + 60, perPlayerAmount);
		}
	}
}
