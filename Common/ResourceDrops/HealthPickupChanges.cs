﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace TerrariaOverhaul.Common.ResourceDrops
{
	public sealed class HealthPickupChanges : ResourcePickupChanges
	{
		public const int HealthPerPickup = 5;

		public static readonly int[] LifeTypes = {
			ItemID.Heart,
			ItemID.CandyApple,
			ItemID.CandyCane
		};

		public override int MaxLifetime => 600;

		public override bool AppliesToEntity(Item item, bool lateInstantiation)
		{
			return LifeTypes.Contains(item.type);
		}

		public override float GetPickupRange(Item item, Player player)
		{
			float range = 160f;

			if (player.lifeMagnet) {
				range *= 2f;
			}

			return range;
		}

		public override bool IsNeededByPlayer(Item item, Player player)
		{
			return player.statLife < player.statLifeMax2;
		}

		public override void OnPickupReal(Item item, Player player)
		{
			int bonus = item.stack * HealthPerPickup;

			player.statLife = Math.Min(player.statLife + bonus, player.statLifeMax2);

			player.HealEffect(bonus);
		}

		public override void PostUpdate(Item item)
		{
			base.PostUpdate(item);

			if (!Main.dedServ) {
				Lighting.AddLight(item.Center, Vector3.UnitX * GetIntensity(item));
			}
		}
	}
}
