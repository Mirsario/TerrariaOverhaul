using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntitySources;

namespace TerrariaOverhaul.Common.ResourceDrops
{
	public sealed class NPCHealthDrops : GlobalNPC
	{
		public static readonly float DefaultHealthDropRange = ResourceDropUtils.DefaultResourceDropRange;

		public override void Load()
		{
			On.Terraria.NPC.NPCLoot += (orig, npc) => {
				orig(npc);

				if (Main.netMode == NetmodeID.Server) {
					return;
				}

				if (npc.damage <= 0 || NPCID.Sets.ProjectileNPC[npc.type]) {
					return;
				}

				var player = Main.LocalPlayer;

				if (player.WithinRange(npc.Center, DefaultHealthDropRange)) {
					DropHealth(npc, player);
				}
			};

			On.Terraria.NPC.NPCLoot_DropHeals += (orig, npc, closestPlayer) => {
				// Juuuust don't.
			};
		}

		public static void DropHealth(NPC npc, Player player, int? amount = null)
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

				dropsCount = ResourceDropUtils.GetDefaultDropCount(player, player.statLife, player.statLifeMax2, HealthPickupChanges.HealthPerPickup, maxDrops, ItemID.Heart);
			}

			IEntitySource entitySource = new EntitySource_EntityResourceDrops(npc);

			for (int i = 0; i < dropsCount; i++) {
				Item.NewItem(entitySource, npc.getRect(), ItemID.Heart, noBroadcast: true);
			}
		}
	}
}
