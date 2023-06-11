using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Npcs.INpcModifyShootProjectile;

namespace TerrariaOverhaul.Common.Hooks.Npcs;

public interface INpcModifyShootProjectile
{
	public static readonly GlobalHookList<GlobalNPC> Hook = NPCLoader.AddModHook(new GlobalHookList<GlobalNPC>(typeof(Hook).GetMethod(nameof(ModifyShootProjectile))));

	void ModifyShootProjectile(NPC npc, ref int type, ref Vector2 position, ref Vector2 velocity, ref int damage, ref float knockback);

	public static void Invoke(NPC npc, ref int type, ref Vector2 position, ref Vector2 velocity, ref int damage, ref float knockback)
	{
		(npc.ModNPC as Hook)?.ModifyShootProjectile(npc, ref type, ref position, ref velocity, ref damage, ref knockback);

		foreach (Hook g in Hook.Enumerate(npc)) {
			g.ModifyShootProjectile(npc, ref type, ref position, ref velocity, ref damage, ref knockback);
		}
	}
}

public sealed class NpcOnShootProjectileImplementation : ILoadable
{
	public void Load(Mod mod)
	{
		On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += (orig, entitySource, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1, ai2) => {
			if (entitySource is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNpc) {
				var position = new Vector2(x, y);
				var velocity = new Vector2(speedX, speedY);

				Hook.Invoke(parentNpc, ref type, ref position, ref velocity, ref damage, ref knockback);

				(x, y, speedX, speedY) = (position.X, position.Y, velocity.X, velocity.Y);
			}

			return orig(entitySource, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1, ai2);
		};
	}

	public void Unload() { }
}
