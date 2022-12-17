using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemProjectileMeleeManaChanges : GlobalItem
{
	public static readonly ConfigEntry<bool> EnableProjectileSwordManaUsage = new(ConfigSide.Both, "Melee", nameof(EnableProjectileSwordManaUsage), () => true);

	private bool useManaOnNextShoot;

	public override bool InstancePerEntity => true;

	public override bool AppliesToEntity(Item item, bool lateInstantiation)
	{
		if (!lateInstantiation) {
			return false;
		}

		// Must not already use mana
		if (item.mana > 0) {
			return false;
		}
		
		// Check the config option
		if (!EnableProjectileSwordManaUsage) {
			return false;
		}

		// Must have actual melee damage
		if (!item.CountsAsClass(DamageClass.Melee) || item.damage <= 0 || item.noMelee) {
			return false;
		}

		// Must have non-channelled projectile firing
		if (item.channel || item.shoot <= ProjectileID.None) {
			return false;
		}

		// Must have sword-like use style
		if (item.useStyle != ItemUseStyleID.Swing) {
			return false;
		}

		return true;
	}

	public override void Load()
	{
		On.Terraria.Player.ItemCheck_PayMana += static  (orig, player, item, canUse) => {
			// Don't pay mana during use, it should instead happen during shooting.
			if (item.TryGetGlobalItem(out ItemProjectileMeleeManaChanges _) && EnableProjectileSwordManaUsage) {
				return true;
			}

			return orig(player, item, canUse);
		};
	}

	public override void SetDefaults(Item item)
	{
		if (!EnableProjectileSwordManaUsage) {
			return;
		}

		item.mana = Math.Max(item.mana, 3 + item.useTime / 5);
	}

	public override bool CanShoot(Item item, Player player)
	{
		if (player.CheckMana(item.mana, pay: false)) {
			useManaOnNextShoot = true;
			
			return true;
		}

		return false;
	}

	public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (useManaOnNextShoot) {
			player.CheckMana(item, pay: true);

			useManaOnNextShoot = false;
		}

		return true;
	}
}
