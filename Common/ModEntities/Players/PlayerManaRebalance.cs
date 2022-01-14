using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.Buffs;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerManaRebalance : ModPlayer
	{
		public static int BaseManaRegen => 13;

		private static bool IsEnabled => true;

		public override void Load()
		{
			// This IL edit completely replaces silly vanilla mana regeneration logic.
			// Forces a constant regeneration value.
			IL.Terraria.Player.UpdateManaRegen += context => {
				var il = new ILCursor(context);

				// manaRegenCount += manaRegen;
				il.GotoNext(
					MoveType.Before,
					i => i.Match(OpCodes.Ldarg_0),
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(Player), nameof(Player.manaRegenCount)),
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(Player), nameof(Player.manaRegen)),
					i => i.Match(OpCodes.Add),
					i => i.MatchStfld(typeof(Player), nameof(Player.manaRegenCount))
				);

				il.GotoNext();
				il.EmitDelegate<Action<Player>>(p => {
					if (IsEnabled) {
						p.manaRegen = BaseManaRegen + p.manaRegenBonus;

						if (p.velocity.Y == 0f && Math.Abs(p.velocity.X) < 2f && p.itemAnimation <= 0 && !p.controlUseItem && p.controlLeft == p.controlRight) {
							p.manaRegen *= 2;

							if (p.statMana < p.statManaMax2) {
								p.AddBuff(ModContent.BuffType<ManaChannelling>(), 2);
							}
						}

						if (p.manaRegenBuff) {
							p.manaRegen *= 2;
						}

						if (p.itemAnimation > 0 && p.HeldItem.mana > 0) {
							p.manaRegen = 0;
						}
					}
				});
				il.Emit(OpCodes.Ldarg_0);
			};
		}
		/*public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
		{
			mult *= 1.3f;
		}*/
	}
}
