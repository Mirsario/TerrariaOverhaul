using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops;

public sealed class HealthPickupChanges : ResourcePickupChanges
{
	public const int HealthPerPickup = 5;
	public const int MaxLifeTime = 600;

	public static readonly int[] LifeTypes = {
		ItemID.Heart,
		ItemID.CandyApple,
		ItemID.CandyCane
	};

	public override int MaxLifetime => MaxLifeTime;

	public override bool AppliesToEntity(Item item, bool lateInstantiation)
	{
		return LifeTypes.Contains(item.type);
	}

	public override float GetPickupRange(Item item, Player player)
	{
		float range = 11f * TileUtils.TileSizeInPixels;

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

	public override void PlayPickupSound(Item item, Player player)
	{
		var sound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Pickups/LifePickup") {
			Volume = 0.33f,
			PitchVariance = 0.15f,
			MaxInstances = 3,
		};

		SoundEngine.PlaySound(sound, !player.IsLocal() ? player.Center : null);
	}

	public override void PostUpdate(Item item)
	{
		item.type = ItemID.Heart; // Disable halloween & christmas styling

		base.PostUpdate(item);

		if (!Main.dedServ) {
			Lighting.AddLight(item.Center, Vector3.UnitX * GetIntensity(item));
		}
	}
}
