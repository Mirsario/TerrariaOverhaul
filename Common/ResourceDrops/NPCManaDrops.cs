using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntitySources;
using TerrariaOverhaul.Common.ModEntities.Projectiles;
using TerrariaOverhaul.Content.Dusts;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops
{
	public sealed class NPCManaDrops : GlobalNPC
	{
		public static readonly float DefaultManaDropRange = ResourceDropUtils.DefaultResourceDropRange;
		public static readonly int ManaDropCooldownTime = 15;

		private int manaDropCooldown = ManaDropCooldownTime;
		private float manaPickupsToDrop;
		private float totalManaPickupsToDrop;

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

			totalManaPickupsToDrop = MathF.Ceiling(totalManaToDrop / ManaPickupChanges.ManaPerPickup);
		}

		public override void PostAI(NPC npc)
		{
			if (Main.netMode != NetmodeID.Server && manaPickupsToDrop >= 1f) {
				if (--manaDropCooldown <= 0) {
					manaDropCooldown = ManaDropCooldownTime;

					var player = Main.LocalPlayer;

					if (player.WithinRange(npc.Center, DefaultManaDropRange)) {
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
				AccumulateManaOnHit(npc, player, damage);
			}
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, int damage, float knockback, bool crit)
		{
			var ownerPlayer = projectile.GetOwner();

			if (ownerPlayer?.IsLocal() == true) {
				AccumulateManaOnHit(npc, ownerPlayer, damage);
			}
		}

		private void AccumulateManaOnHit(NPC npc, Player player, float damage)
		{
			if (player.statMana >= player.statManaMax2) {
				return;
			}

			float effectiveDamage = Math.Max(0, damage + Math.Min(0, npc.life));

			manaPickupsToDrop += effectiveDamage / Math.Max(npc.lifeMax, 1f) * totalManaPickupsToDrop;

			// Drop everything instantly if dead.
			if (!npc.active) {
				DropMana(npc, player, (int)Math.Floor(manaPickupsToDrop));

				manaPickupsToDrop = 0;
			}
		}

		public static void DropMana(NPC npc, Player player, int? amount = null)
		{
			int dropsCount;

			if (amount.HasValue) {
				dropsCount = amount.Value;
			} else {
				dropsCount = ResourceDropUtils.GetDefaultDropCount(player, player.statMana, player.statManaMax2, ManaPickupChanges.ManaPerPickup, 3);
			}

			IEntitySource entitySource = new EntitySource_EntityResourceDrops(npc);

			for (int i = 0; i < dropsCount; i++) {
				Item.NewItem(entitySource, npc.getRect(), ItemID.Star, noBroadcast: true);
			}
		}
	}
}
