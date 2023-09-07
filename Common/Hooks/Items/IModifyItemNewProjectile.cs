using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemNewProjectile;

namespace TerrariaOverhaul.Common.Hooks.Items;

public interface IModifyItemNewProjectile
{
	//TODO: Use ref fields when they are available.
	public ref struct Args
	{
		public IEntitySource Source;
		public Vector2 Position;
		public Vector2 Velocity;
		public int Type;
		public int Damage;
		public float Knockback;
		public int Owner;
		public float AI0;
		public float AI1;
		public float AI2;
	}

	public static readonly GlobalHookList<GlobalItem> Hook = ItemLoader.AddModHook(new GlobalHookList<GlobalItem>(typeof(Hook).GetMethod(nameof(ModifyShootProjectile))));

	void ModifyShootProjectile(Player player, Item item, in Args args, ref Args result);

	public static void Invoke(Player player, Item item, in Args args, ref Args result)
	{
		(item.ModItem as Hook)?.ModifyShootProjectile(player, item, in args, ref result);

		foreach (Hook g in Hook.Enumerate(item)) {
			g.ModifyShootProjectile(player, item, in args, ref result);
		}
	}
}

public sealed class ItemOnShootProjectileImplementation : ILoadable
{
	public void Load(Mod mod)
	{
		On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += (orig, entitySource, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1, ai2) => {
			if (entitySource is EntitySource_ItemUse_WithAmmo parentSource && parentSource.Entity is Player player) {
				Hook.Args data;

				data.Source = entitySource;
				data.Position = new Vector2(x, y);
				data.Velocity = new Vector2(speedX, speedY);
				data.Type = type;
				data.Damage = damage;
				data.Knockback = knockback;
				data.Owner = owner;
				data.AI0 = ai0;
				data.AI1 = ai1;
				data.AI2 = ai2;

				var args = data;

				Hook.Invoke(player, parentSource.Item, in args, ref data);

				(x, y, speedX, speedY, ai0, ai1, ai2) = (data.Position.X, data.Position.Y, data.Velocity.X, data.Velocity.Y, data.AI0, data.AI1, data.AI2);
			}

			return orig(entitySource, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1, ai2);
		};
	}

	public void Unload() { }
}
