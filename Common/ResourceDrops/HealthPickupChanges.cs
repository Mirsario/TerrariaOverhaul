using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops;

public sealed class HealthPickupChanges : ResourcePickupChanges<HealthPickupChanges>
{
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
			PickupSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Pickups/LifePickup") {
				Volume = 0.33f,
				PitchVariance = 0.15f,
				MaxInstances = 3,
			};

			TextureOverride = Mod.Assets.Request<Texture2D>("Common/ResourceDrops/LifeEssence");
		}
	}

	public override bool AppliesToEntity(Item item, bool lateInstantiation)
	{
		return LifeTypes.Contains(item.type);
	}

	public override float GetPickupRange(Item item, Player player)
	{
		float range = 8f * TileUtils.TileSizeInPixels;

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
