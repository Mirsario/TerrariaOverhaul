using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace TerrariaOverhaul.Common.ModEntities.Items
{
	public sealed class ManaPickupChanges : ResourcePickupChanges
	{
		public const int ManaPerPickup = 5;

		public static readonly int[] ManaTypes = {
			ItemID.Star,
			ItemID.SoulCake,
			ItemID.SugarPlum
		};

		public override int MaxLifetime => 300;

		public override bool AppliesToEntity(Item item, bool lateInstantiation)
		{
			return ManaTypes.Contains(item.type);
		}

		public override void PostUpdate(Item item)
		{
			base.PostUpdate(item);

			if (!Main.dedServ) {
				Lighting.AddLight(item.Center, Vector3.UnitX * GetIntensity(item));
			}
		}

		public override void OnPickupReal(Item item, Player player)
		{
			int bonus = item.stack * ManaPerPickup;

			player.statMana = Math.Min(player.statMana + bonus, player.statManaMax2);

			player.ManaEffect(bonus);
		}

		public override float GetPickupRange(Item item, Player player)
		{
			float range = 192f;

			if (player.lifeMagnet) {
				range *= 2f;
			}

			return range;
		}

		public override bool IsNeededByPlayer(Item item, Player player)
		{
			return player.statMana < player.statManaMax2;
		}
	}
}
