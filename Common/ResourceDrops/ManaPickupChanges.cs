using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.Dusts;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops;

public sealed class ManaPickupChanges : ResourcePickupChanges<ManaPickupChanges>
{
	public const int ManaPerPickup = 5;

	public static readonly int[] ManaTypes = {
		ItemID.Star,
		ItemID.SoulCake,
		ItemID.SugarPlum
	};

	public override void Load()
	{
		base.Load();

		MaxLifeTime = 600;
		ForcedItemType = ItemID.Star;

		if (!Main.dedServ) {
			LightColor = new Vector3(0f, 0f, 1f);
			PickupSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Pickups/ManaPickup") {
				Volume = 0.275f,
				PitchVariance = 0.15f,
				MaxInstances = 3,
			};

			TextureOverride = Mod.Assets.Request<Texture2D>("Common/ResourceDrops/ManaEssence");
		}
	}

	public override bool AppliesToEntity(Item item, bool lateInstantiation)
	{
		return ManaTypes.Contains(item.type);
	}

	public override void ApplyPickupEffect(Item item, Player player)
	{
		int bonus = item.stack * ManaPerPickup;

		player.statMana = Math.Min(player.statMana + bonus, player.statManaMax2);

		player.ManaEffect(bonus);
	}

	public override float GetPickupRange(Item item, Player player)
	{
		float range = 12f * TileUtils.TileSizeInPixels;

		if (player.lifeMagnet) {
			range *= 2f;
		}

		return range;
	}

	public override bool IsNeededByPlayer(Item item, Player player)
	{
		return player.statMana < player.statManaMax2;
	}

	public override void SpawnDust(Item item, int amount, Rectangle rectangle, Func<Vector2> velocityGetter)
	{
		int dustType = ModContent.DustType<ManaDust>();

		for (int i = 0; i < amount; i++) {
			var dust = Dust.NewDustDirect(rectangle.TopLeft(), rectangle.Width, rectangle.Height, dustType, Scale: Main.rand.NextFloat(1.5f, 2f));

			dust.noLight = true;
			dust.noGravity = true;
			dust.velocity = velocityGetter();
			dust.alpha = 32;
		}
	}
}
