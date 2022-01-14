using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanDoMeleeDamage;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface ICanDoMeleeDamage
	{
		public delegate bool Delegate(Item item, Player player);

		public static readonly HookList<GlobalItem, Delegate> Hook = ItemLoader.AddModHook(new HookList<GlobalItem, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(CanDoMeleeDamage)),
			// Invocation
			e => (Item item, Player player) => {
				foreach (Hook g in e.Enumerate(item)) {
					if (!g.CanDoMeleeDamage(item, player)) {
						return false;
					}
				}

				return true;
			}
		));

		bool CanDoMeleeDamage(Item item, Player player);
	}

	public sealed class CanDoMeleeDamageImplementation : GlobalItem
	{
		public override void Load()
		{
			On.Terraria.Player.ItemCheck_MeleeHitNPCs += (orig, player, item, itemRectangle, originalDamage, knockback) => {
				if (Hook.Hook.Invoke(item, player)) {
					orig(player, item, itemRectangle, originalDamage, knockback);
				}
			};
		}
	}
}
