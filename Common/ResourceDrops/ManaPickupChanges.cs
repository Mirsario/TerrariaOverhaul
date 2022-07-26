using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops;

public sealed class ManaPickupChanges : ResourcePickupChanges
{
	public const int ManaPerPickup = 5;
	public const int MaxLifeTime = 600;

	public static readonly int[] ManaTypes = {
		ItemID.Star,
		ItemID.SoulCake,
		ItemID.SugarPlum
	};

	public override int MaxLifetime => MaxLifeTime;

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
		float range = 16f * TileUtils.TileSizeInPixels;

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
