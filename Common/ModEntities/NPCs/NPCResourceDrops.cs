using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items;
using TerrariaOverhaul.Common.ModEntities.Projectiles;
using TerrariaOverhaul.Common.Systems.Time;
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

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			On.Terraria.NPC.NPCLoot_DropHeals += (orig, npc, closestPlayer) => {
				if(Main.netMode == NetmodeID.Server) {
					return;
				}

				if(npc.damage <= 0) {
					return;
				}

				var player = Main.LocalPlayer;

				if(player.WithinRange(npc.Center, DefaultResourceDropRange)) {
					DropHealth(npc, player);
					//DropMana(npc, player);
				}
			};
		}
		public override void PostAI(NPC npc)
		{
			if(Main.netMode != NetmodeID.Server && manaPickupsToDrop >= 1f) {
				if(--manaDropCooldown <= 0) {
					manaDropCooldown = ManaDropCooldownTime;

					var player = Main.LocalPlayer;

					if(player.WithinRange(npc.Center, DefaultResourceDropRange)) {
						DropMana(npc, player, 1);

						manaPickupsToDrop--;
					}
				}

				if(!Main.dedServ) {
					Lighting.AddLight(npc.Center, Color.BlueViolet.ToVector3() * ((float)Math.Sin(Main.GameUpdateCount / 60f * 4f) * 0.5f + 0.5f));

					if(Main.GameUpdateCount % 2 == 0) {
						Vector2 point = npc.getRect().GetRandomPoint();
						//Vector2 directionTowardsCenter = point.DirectionTo(npc.Center);

						Dust.NewDustPerfect(point, ModContent.DustType<ManaDust>(), Vector2.Zero);
					}
				}
			}
		}
		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit)
		{
			if(player.IsLocal() && item.CountsAsClass(DamageClass.Magic)) {
				OnDamagedByMagic(npc, player, damage, item.useTime, item.useAnimation);
			}
		}
		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
			var ownerPlayer = projectile.GetOwner();

			if(ownerPlayer?.IsLocal() == true && projectile.CountsAsClass(DamageClass.Magic)
			&& projectile.TryGetGlobalProjectile<ProjectileSourceItemInfo>(out var projInfo) && projInfo.Available) {
				OnDamagedByMagic(npc, ownerPlayer, damage, projInfo.UseTime, projInfo.UseAnimation);
			}
		}

		public void DropHealth(NPC npc, Player player, int? amount = null)
		{
			int dropsCount;

			if(amount.HasValue) {
				dropsCount = amount.Value;
			} else {
				dropsCount = GetDefaultDropCount(player, player.statLife, player.statLifeMax2, ManaPickupChanges.ManaPerPickup, 3, ItemID.Heart);
			}

			for(int i = 0; i < dropsCount; i++) {
				Item.NewItem(npc.getRect(), ItemID.Heart, noBroadcast: true);
			}
		}
		public void DropMana(NPC npc, Player player, int? amount = null)
		{
			int dropsCount;

			if(amount.HasValue) {
				dropsCount = amount.Value;
			} else {
				dropsCount = GetDefaultDropCount(player, player.statMana, player.statManaMax2, ManaPickupChanges.ManaPerPickup, 3);
			}
			
			for(int i = 0; i < dropsCount; i++) {
				Item.NewItem(npc.getRect(), ItemID.Star, noBroadcast: true);
			}
		}

		private int GetDefaultDropCount(Player player, int statCurrent, int statMax, int statGainPerPickup, int? maxDrops = null, int? countedItemId = null, float? countingRangeOverride = null)
		{
			int neededHealth = statMax - statCurrent;
			int neededDrops = (int)Math.Ceiling(neededHealth / (float)statGainPerPickup);
			int dropsCount = neededDrops;

			if(countedItemId.HasValue) {
				int id = countedItemId.Value;
				float checkRange = countingRangeOverride ?? DefaultResourceDropRange;
				int existingDrops = Main.item.Count(i => i?.active == true && i.type == id && player.WithinRange(i.position, checkRange));

				dropsCount = Math.Max(0, dropsCount - existingDrops);
			}

			if(maxDrops.HasValue) {
				dropsCount = Math.Min(maxDrops.Value, dropsCount);
			}

			return dropsCount;
		}
		private void OnDamagedByMagic(NPC npc, Player player, float damage, int itemUseTime, int itemUseAnimation)
		{
			const float TickToManaFactor = TimeSystem.LogicDeltaTime / 3f; //Drop 1 mana per X seconds at average
			const float TickToManaPickupFactor = TickToManaFactor * ManaPickupChanges.ManaPerPickup;

			manaPickupsToDrop += itemUseTime * TickToManaPickupFactor;

			//Drop everything instantly if dead.
			if(!npc.active) {
				DropMana(npc, player, (int)Math.Ceiling(manaPickupsToDrop));

				manaPickupsToDrop = 0;
			}
		}
	}
}
