using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using Hook = TerrariaOverhaul.Common.Hooks.Npcs.INpcModifyShootProjectile;

namespace TerrariaOverhaul.Common.Hooks.Npcs
{
	public interface INpcModifyShootProjectile
	{
		public delegate void Delegate(NPC npc, ref int type, ref Vector2 position, ref Vector2 velocity, ref int damage, ref float knockback);

		public static readonly HookList<GlobalNPC, Delegate> Hook = NPCLoader.AddModHook(new HookList<GlobalNPC, Delegate>(
			// Method reference
			typeof(Hook).GetMethod(nameof(ModifyShootProjectile)),
			// Invocation
			e => (NPC npc, ref int type, ref Vector2 position, ref Vector2 velocity, ref int damage, ref float knockback) => {
				foreach(Hook g in e.Enumerate(npc)) {
					g.ModifyShootProjectile(npc, ref type, ref position, ref velocity, ref damage, ref knockback);
				}
			}
		));

		void ModifyShootProjectile(NPC npc, ref int type, ref Vector2 position, ref Vector2 velocity, ref int damage, ref float knockback);
	}

	public sealed class NpcOnShootProjectileImplementation : ILoadable
	{
		public void Load(Mod mod)
		{
			On.Terraria.Projectile.NewProjectile_IProjectileSource_float_float_float_float_int_int_float_int_float_float += (orig, projectileSource, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1) => {
				if(projectileSource is ProjectileSource_NPC npcSource) {
					var position = new Vector2(x, y);
					var velocity = new Vector2(speedX, speedY);

					Hook.Hook.Invoke(npcSource.NPC, ref type, ref position, ref velocity, ref damage, ref knockback);

					(x, y, speedX, speedY) = (position.X, position.Y, velocity.X, velocity.Y);
				}

				return orig(projectileSource, x, y, speedX, speedY, type, damage, knockback, owner, ai0, ai1);
			};
		}

		public void Unload() { }
	}
}
