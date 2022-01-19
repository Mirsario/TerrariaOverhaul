using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components.Melee
{
	public sealed class ItemMeleeCooldownDisabler : ItemComponent
	{
		public override void Load()
		{
			// Disable attackCD for melee whenever this component is present on the held item and enabled.
			IL.Terraria.Player.ItemCheck_MeleeHitNPCs += context => {
				var c = new ILCursor(context);

				// attackCD = Math.Max(1, (int)((double)itemAnimationMax * 0.33));
				c.GotoNext(
					MoveType.Before,
					i => i.Match(OpCodes.Ldarg_0),
					i => i.Match(OpCodes.Ldc_I4_1),
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(Player), nameof(Player.itemAnimationMax)),
					i => i.Match(OpCodes.Conv_R8),
					i => i.MatchLdcR8(0.33d),
					i => i.Match(OpCodes.Mul),
					i => i.Match(OpCodes.Conv_I4),
					i => i.MatchCall(typeof(Math), nameof(Math.Max)),
					i => i.MatchStfld(typeof(Player), nameof(Player.attackCD))
				);

				var jumpLabel = c.DefineLabel();

				c.Emit(OpCodes.Ldarg_0); // Load 'this' (player)
				c.EmitDelegate<Func<Player, bool>>(p => p.HeldItem?.IsAir == false && p.HeldItem.TryGetGlobalItem(out ItemMeleeCooldownDisabler disabler) && disabler.Enabled);
				c.Emit(OpCodes.Brtrue, jumpLabel);

				c.Index += 10;

				c.MarkLabel(jumpLabel);
			};
		}
	}
}
