using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops
{
	public static class ResourceDropUtils
	{
		public static readonly float DefaultResourceDropRange = 1280f;

		public static int GetResourceDropsNeededByPlayer(int statCurrent, int statMax, int statGainPerPickup)
		{
			int neededStats = statMax - statCurrent;
			int neededDrops = (int)Math.Ceiling(neededStats / (float)statGainPerPickup);

			return neededDrops;
		}

		public static int CountItemsOfTypeWithinRange(int itemType, Vector2 position, float range)
		{
			int itemCount = 0;
			float rangeSquared = range * range;

			foreach (var item in ActiveEntities.Items) {
				if (item.type == itemType && Vector2.DistanceSquared(position, item.position) <= rangeSquared) {
					itemCount++;
				}
			}

			return itemCount;
		}

		public static void DropResource(IEntitySource entitySource, Vector2 position, int type, int maxAmount, int maxExpectedLifeTime, Dictionary<Player, int>? perPlayerAmount = null)
		{
			for (int i = 0; i < maxAmount; i++) {
				IEnumerable<Player>? players = null;

				if (perPlayerAmount != null) {
					players = perPlayerAmount.Where(pair => pair.Value > i).Select(pair => pair.Key);
				}

				ItemUtils.NewItemInstanced(entitySource, position, type, players: players, maxExpectedLifeTime: maxExpectedLifeTime);
			}
		}
	}
}
