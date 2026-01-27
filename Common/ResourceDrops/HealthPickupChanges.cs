// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.ResourceDrops;

internal sealed class HealthPickupChanges : ResourcePickupChanges<HealthPickupChanges>
{
	public static readonly ConfigEntry<bool> EnableHealthDropsRework = new(ConfigSide.Both, true, "Balance");
	public static readonly ConfigEntry<bool> EnableHealthPickupSounds = new(ConfigSide.ClientOnly, true, "Awareness");

	public const int HealthPerPickup = 5;

	public static readonly int[] LifeTypes = {
		ItemID.Heart,
		ItemID.CandyApple,
		ItemID.CandyCane
	};

	public override void Load()
	{
		base.Load();

		MaxLifeTime = 600;
		ForcedItemType = ItemID.Heart;

		if (!Main.dedServ) {
			LightColor = new Vector3(1f, 0f, 0f);
			PickupSound = !EnableHealthPickupSounds ? SoundID.Item4 : new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Pickups/LifePickup") {
				Volume = 0.33f,
				PitchVariance = 0.15f,
				MaxInstances = 3,
			};

			TextureOverride = Mod.Assets.Request<Texture2D>("Common/ResourceDrops/LifeEssence");
		}
	}

	public override bool AppliesToEntity(Item item, bool lateInstantiation)
	{
		return EnableHealthDropsRework && LifeTypes.Contains(item.type);
	}

	public override float GetPickupRange(Item item, Player player)
	{
		float range = 8f * WorldUtils.TileSizeInPixels;

		if (player.lifeMagnet) {
			range *= 2f;
		}

		return range;
	}

	public override bool IsNeededByPlayer(Item item, Player player)
	{
		return player.statLife < player.statLifeMax2;
	}

	public override void ApplyPickupEffect(Item item, Player player)
	{
		int bonus = item.stack * HealthPerPickup;

		player.statLife = Math.Min(player.statLife + bonus, player.statLifeMax2);

		player.HealEffect(bonus);
	}

	public override void SpawnDust(Item item, int amount, Rectangle rectangle, Func<Vector2> velocityGetter)
	{
		for (int i = 0; i < amount; i++) {
			var dust = Dust.NewDustDirect(rectangle.TopLeft(), rectangle.Width, rectangle.Height, DustID.SomethingRed, Scale: Main.rand.NextFloat(1.5f, 2f));

			dust.noLight = true;
			dust.noGravity = true;
			dust.velocity = velocityGetter();
			dust.alpha = 96;
		}
	}
}
