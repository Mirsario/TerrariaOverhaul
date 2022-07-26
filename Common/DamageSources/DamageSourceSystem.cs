using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.DamageSources;

// This weird system tries to guess damage sources based on callstack locations.
// Used in places like StrikeNPC hooks.
public class DamageSourceSystem : ModSystem
{
	public static DamageSource? CurrentDamageSource { get; private set; }

	public override void Load()
	{
		On.Terraria.Player.ItemCheck_MeleeHitNPCs += (orig, player, item, itemRectangle, originalDamage, knockback) => {
			var oldSource = CurrentDamageSource;
			CurrentDamageSource = new DamageSource(item, new DamageSource(player));

			orig(player, item, itemRectangle, originalDamage, knockback);

			CurrentDamageSource = oldSource;
		};
		On.Terraria.Player.ItemCheck_MeleeHitPVP += (orig, player, item, itemRectangle, originalDamage, knockback) => {
			var oldSource = CurrentDamageSource;
			CurrentDamageSource = new DamageSource(item, new DamageSource(player));

			orig(player, item, itemRectangle, originalDamage, knockback);

			CurrentDamageSource = oldSource;
		};
		On.Terraria.Projectile.Damage += (orig, projectile) => {
			var oldSource = CurrentDamageSource;
			var owner = projectile.GetOwner();

			CurrentDamageSource = new DamageSource(projectile, owner != null ? new DamageSource(owner) : null);

			orig(projectile);

			CurrentDamageSource = oldSource;
		};
	}

	public override void Unload()
	{
		CurrentDamageSource = null;
	}

	public override void PostUpdateEverything()
	{
		CurrentDamageSource = null; // Reset just in case exceptions screw something over.
	}
}
