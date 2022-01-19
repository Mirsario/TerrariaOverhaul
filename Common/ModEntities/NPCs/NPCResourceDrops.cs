using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntitySources;
using TerrariaOverhaul.Common.ModEntities.Items;
using TerrariaOverhaul.Common.ModEntities.Projectiles;
using TerrariaOverhaul.Content.Dusts;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCResourceDrops : GlobalNPC
	{
		public const int ManaDropCooldownTime = 15;
		public const float DefaultResourceDropRange = 1280f;

		private int manaDropCooldown = ManaDropCooldownTime;
		private float manaPickupsToDrop;
		private float totalManaPickupsToDrop;

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			On.Terraria.NPC.NPCLoot_DropHeals += (orig, npc, closestPlayer) => {
				if (Main.netMode == NetmodeID.Server) {
					return;
				}

				if (npc.damage <= 0 || NPCID.Sets.ProjectileNPC[npc.type]) {
					return;
				}

				var player = Main.LocalPlayer;

				if (player.WithinRange(npc.Center, DefaultResourceDropRange)) {
					DropHealth(npc, player);
					//DropMana(npc, player);
				}
			};
		}

		public override void SetDefaults(NPC npc)
		{
			if (npc.damage <= 0 || NPCID.Sets.ProjectileNPC[npc.type]) {
				return;
			}

			float totalManaToDrop = 10f;

			if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type]) {
				totalManaToDrop = npc.lifeMax / 75f; // This is so not going to be balanced...
			}

			totalManaPickupsToDrop = MathF.Ceiling(totalManaToDrop / ManaPickupChanges.ManaPerPickup);
		}

		public override void PostAI(NPC npc)
		{
			if (Main.netMode != NetmodeID.Server && manaPickupsToDrop >= 1f) {
				if (--manaDropCooldown <= 0) {
					manaDropCooldown = ManaDropCooldownTime;

					var player = Main.LocalPlayer;

					if (player.WithinRange(npc.Center, DefaultResourceDropRange)) {
						DropMana(npc, player, 1);

						manaPickupsToDrop--;
					}
				}

				if (!Main.dedServ) {
					Lighting.AddLight(npc.Center, Color.BlueViolet.ToVector3() * ((float)Math.Sin(Main.GameUpdateCount / 60f * 4f) * 0.5f + 0.5f));

					if (Main.GameUpdateCount % 2 == 0) {
						Vector2 point = npc.getRect().GetRandomPoint();
						IEntitySource entitySource = new EntitySource_EntityResourceDrops(npc);

						Dust.NewDustPerfect(entitySource, point, ModContent.DustType<ManaDust>(), Vector2.Zero);
					}
				}
			}
		}

		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		{
			if (player.IsLocal()) {
				AccumulateManaOnHit(npc, player, damage, item.useTime, item.useAnimation, item.mana);
			}
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
			var ownerPlayer = projectile.GetOwner();

			if (ownerPlayer?.IsLocal() == true && projectile.TryGetGlobalProjectile<ProjectileSourceItemInfo>(out var projInfo) && projInfo.Available) {
				AccumulateManaOnHit(npc, ownerPlayer, damage, projInfo.UseTime, projInfo.UseAnimation, projInfo.ManaUse);
			}
		}

		public void DropHealth(NPC npc, Player player, int? amount = null)
		{
			int dropsCount;

			if (amount.HasValue) {
				dropsCount = amount.Value;
			} else {
				float healthFactor = player.statLife / (float)player.statLifeMax2;
				int maxDrops;

				if (healthFactor <= 0.25f) {
					maxDrops = 3;
				} else {
					maxDrops = 1;
				}

				dropsCount = GetDefaultDropCount(player, player.statLife, player.statLifeMax2, HealthPickupChanges.HealthPerPickup, maxDrops, ItemID.Heart);
			}

			IEntitySource entitySource = new EntitySource_EntityResourceDrops(npc);

			for (int i = 0; i < dropsCount; i++) {
				Item.NewItem(entitySource, npc.getRect(), ItemID.Heart, noBroadcast: true);
			}
		}

		public void DropMana(NPC npc, Player player, int? amount = null)
		{
			int dropsCount;

			if (amount.HasValue) {
				dropsCount = amount.Value;
			} else {
				dropsCount = GetDefaultDropCount(player, player.statMana, player.statManaMax2, ManaPickupChanges.ManaPerPickup, 3);
			}

			IEntitySource entitySource = new EntitySource_EntityResourceDrops(npc);

			for (int i = 0; i < dropsCount; i++) {
				Item.NewItem(entitySource, npc.getRect(), ItemID.Star, noBroadcast: true);
			}
		}

		private int GetDefaultDropCount(Player player, int statCurrent, int statMax, int statGainPerPickup, int? maxDrops = null, int? countedItemId = null, float? countingRangeOverride = null)
		{
			int neededHealth = statMax - statCurrent;
			int neededDrops = (int)Math.Ceiling(neededHealth / (float)statGainPerPickup);
			int dropsCount = neededDrops;

			if (countedItemId.HasValue) {
				int id = countedItemId.Value;
				float checkRange = countingRangeOverride ?? DefaultResourceDropRange;
				int existingDrops = Main.item.Count(i => i?.active == true && i.type == id && player.WithinRange(i.position, checkRange));

				dropsCount = Math.Max(0, dropsCount - existingDrops);
			}

			if (maxDrops.HasValue) {
				dropsCount = Math.Min(maxDrops.Value, dropsCount);
			}

			return dropsCount;
		}

		private void AccumulateManaOnHit(NPC npc, Player player, float damage, int useTime, int useAnimation, int manaUse)
		{
			if (player.statMana >= player.statManaMax2) {
				return;
			}

			/*
			const float ManaUsePerSecondToManaFactor = 1f / 15f;
			const float ManaUsePerSecondToManaPickupFactor = ManaUsePerSecondToManaFactor / ManaPickupChanges.ManaPerPickup;

			//manaUse = 10;

			float manaUsePerSecond = manaUse / (useAnimation * TimeSystem.LogicDeltaTime);
			int shotsPerUse = Math.Max(1, useAnimation / useTime);

			manaPickupsToDrop += manaUsePerSecond * ManaUsePerSecondToManaPickupFactor / shotsPerUse;
			*/

			float effectiveDamage = Math.Max(0, damage + Math.Min(0, npc.life));

			manaPickupsToDrop += effectiveDamage / Math.Max(npc.lifeMax, 1f) * totalManaPickupsToDrop;
			//manaPickupsToDrop += useTime / ManaPickupChanges.ManaPerPickup / 6f;
			//manaPickupsToDrop += dps / ManaPickupChanges.ManaPerPickup * 2f;

			// Drop everything instantly if dead.
			if (!npc.active) {
				DropMana(npc, player, (int)Math.Floor(manaPickupsToDrop));

				manaPickupsToDrop = 0;
			}
		}
	}
}
