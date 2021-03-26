using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCResourceDrops : GlobalNPC
	{
		public static Gradient<float> DropsCountByCurrentAmount { get; private set; }

		public override void Load()
		{
			DropsCountByCurrentAmount = new Gradient<float>(
				(0.0f, 3f),
				(0.5f, 1f),
				(1.0f, 1f)
			);

			On.Terraria.NPC.NPCLoot_DropHeals += (orig, npc, closestPlayer) => {
				if(Main.netMode == NetmodeID.Server) {
					return;
				}

				const float MaxDistanceSquared = 1024f * 1024f;

				var player = Main.LocalPlayer;

				if(Vector2.DistanceSquared(player.Center, npc.Center) > MaxDistanceSquared) {
					return;
				}

				float healthFactor = player.statLife / (float)player.statLifeMax2;

				if(healthFactor < 1f) {
					int dropsCount = (int)Math.Round(DropsCountByCurrentAmount.GetValue(healthFactor));

					for(int i = 0; i < dropsCount; i++) {
						Item.NewItem(npc.getRect(), ItemID.Heart, noBroadcast: true);
					}
				}

				float manaFactor = player.statMana / (float)player.statManaMax2;

				if(manaFactor < 1f) {
					int dropsCount = (int)Math.Round(DropsCountByCurrentAmount.GetValue(manaFactor));

					for(int i = 0; i < dropsCount; i++) {
						Item.NewItem(npc.getRect(), ItemID.Star, noBroadcast: true);
					}
				}
			};
		}
		public override void Unload()
		{
			DropsCountByCurrentAmount = null;
		}
	}
}
