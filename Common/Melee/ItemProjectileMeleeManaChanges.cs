using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Melee
{
	public sealed class ItemProjectileMeleeManaChanges : GlobalItem
	{
		public override bool AppliesToEntity(Item item, bool lateInstantiation)
		{
			if (!lateInstantiation) {
				return false;
			}

			// Must not already use mana
			if (item.mana > 0) {
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

			return true;
		}

		public override void Load()
		{
			On.Terraria.Player.ItemCheck_PayMana += (orig, player, item, canUse) => {
				if (item.TryGetGlobalItem(out ItemProjectileMeleeManaChanges _)) {
					return true;
				}

				return orig(player, item, canUse);
			};

			IL.Terraria.Player.ItemCheck_Shoot += context => {
				var c = new ILCursor(context);
				ILLabel skipReturn = null;

				c.GotoNext(
					MoveType.After,
					i => i.MatchLdarg(0),
					i => i.MatchLdarg(2),
					i => i.MatchCall(typeof(CombinedHooks), nameof(CombinedHooks.CanShoot)),
					i => i.MatchBrtrue(out skipReturn)
				);

				// Don't return if mana consumption succeeds

				c.Index--; // Step back to before Brtrue

				var returnLabel = c.DefineLabel();

				c.Emit(OpCodes.Brfalse, returnLabel);
				c.Emit(OpCodes.Ldarg_0);
				c.Emit(OpCodes.Ldarg_2);
				c.Emit(OpCodes.Call, typeof(ItemProjectileMeleeManaChanges).GetMethod(nameof(CanReallyShoot), BindingFlags.Static | BindingFlags.NonPublic));

				c.Index++; // Step over back onto Brtrue, with Ret(urn) in front of us

				c.MarkLabel(returnLabel);
			};
		}

		public override void SetDefaults(Item item)
		{
			item.mana = Math.Max(item.mana, 3 + item.useTime / 6);
		}

		private static bool CanReallyShoot(Player player, Item item)
		{
			if (item.TryGetGlobalItem(out ItemProjectileMeleeManaChanges _)) {
				return player.CheckMana(item.mana, pay: true);
			}

			return true;
		}
	}
}
