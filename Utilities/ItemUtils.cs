using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace TerrariaOverhaul.Utilities
{
	public static class ItemUtils
	{
		public static void NewItemInstanced(IEntitySource source, Vector2 position, int type, int stack = 1, IEnumerable<Player>? players = null, int maxExpectedLifeTime = 54000, int prefix = 0)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				throw new InvalidOperationException($"{nameof(NewItemInstanced)} must not be called on multiplayer clients.");
			}

			int itemId = Item.NewItem(source, position, type, stack, true, prefix);

			if (Main.netMode == NetmodeID.Server) {
				players ??= ActiveEntities.Players;

				Main.timeItemSlotCannotBeReusedFor[itemId] = maxExpectedLifeTime;

				foreach (var player in players) {
					NetMessage.SendData(MessageID.InstancedItem, player.whoAmI, -1, null, itemId);
				}

				Main.item[itemId].active = false;
			}
		}
	}
}
