using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using TerrariaOverhaul.Utilities.DataStructures;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyCommonStatMultipliers;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyCommonStatMultipliers
	{
		public delegate void Delegate(Item item, Player player, ref CommonStatMultipliers multipliers);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			//Method reference
			typeof(Hook).GetMethod(nameof(ModifyCommonStatMultipliers)),
			//Invocation
			e => (Item item, Player player, ref CommonStatMultipliers multipliers) => {
				(item.ModItem as Hook)?.ModifyCommonStatMultipliers(item, player, ref multipliers);

				foreach (Hook g in e.Enumerate(item)) {
					g.ModifyCommonStatMultipliers(item, player, ref multipliers);
				}
			}
		));

		void ModifyCommonStatMultipliers(Item item, Player player, ref CommonStatMultipliers multipliers);

		public static CommonStatMultipliers GetMultipliers(Item item, Player player)
		{
			var multipliers = CommonStatMultipliers.Default;

			Hook.Invoke(item, player, ref multipliers);

			return multipliers;
		}
	}

	public sealed class ItemModifyCommonStatMultipliersImplementation : GlobalItem, IModifyItemMeleeRange
	{
		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			var multipliers = Hook.GetMultipliers(item, player);

			damage = (int)(damage * multipliers.MeleeDamageMultiplier);
			knockback *= multipliers.MeleeKnockbackMultiplier;
		}

		public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			var multipliers = Hook.GetMultipliers(item, player);

			damage = (int)(damage * multipliers.ProjectileDamageMultiplier);
			knockback *= multipliers.ProjectileKnockbackMultiplier;
			velocity *= multipliers.ProjectileSpeedMultiplier;
		}

		void IModifyItemMeleeRange.ModifyMeleeRange(Item item, Player player, ref float range)
		{
			var multipliers = Hook.GetMultipliers(item, player);

			range *= multipliers.MeleeRangeMultiplier;
		}
	}
}
