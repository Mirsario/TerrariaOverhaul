using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Guns;

[ItemAttachment(ItemID.Revolver, ItemID.TheUndertaker)]
public class Revolver : ItemOverhaul
{
	//public static readonly SoundStyle RevolverFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Revolver/RevolverFire") {
	public static readonly SoundStyle RevolverFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Handgun/HandgunFire") {
		Volume = 0.15f,
		PitchVariance = 0.2f,
	};

	public const float SpinAnimationLengthMultiplier = 2f;
	public const int SpinShotCount = 6;

	//TODO: Implement rules, be sure to differentiate from handguns somehow.
	public override bool ShouldApplyItemOverhaul(Item item) => false;

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		item.UseSound = RevolverFireSound;

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
			item.EnableComponent<ItemPlaySoundOnEveryUse>();

			item.EnableComponent<ItemBulletCasings>(c => {
				c.CasingGoreType = ModContent.GoreType<BulletCasing>();
			});
		}
	}

	//TODO: Move all the following logic to a component

	public override bool AltFunctionUse(Item item, Player player)
	{
		return true;
	}

	public override float UseTimeMultiplier(Item item, Player player)
	{
		if (player.altFunctionUse == 2) {
			return 1f / (SpinShotCount - 1);
		}

		return base.UseTimeMultiplier(item, player);
	}

	public override float UseSpeedMultiplier(Item item, Player player)
	{
		if (player.altFunctionUse == 2) {
			return 0.6f;
		}

		return base.UseTimeMultiplier(item, player);
	}

	public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		base.ModifyShootStats(item, player, ref position, ref velocity, ref type, ref damage, ref knockback);

		if (player.altFunctionUse == 2) {
			velocity = velocity.RotatedByRandom(MathHelper.ToRadians(12f));
			damage = (int)(damage * 0.75f);
		}
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (player.altFunctionUse == 2) {
			player.reuseDelay = Math.Max(player.reuseDelay, item.useAnimation * 2);
		}

		return base.UseItem(item, player);
	}
}
