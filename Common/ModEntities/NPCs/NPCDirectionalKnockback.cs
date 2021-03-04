using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCDirectionalKnockback : GlobalNPC
	{
		private Vector2? knockbackDirection;

		public override bool InstancePerEntity => true;

		public override void Load()
		{
			IL.Terraria.NPC.StrikeNPC += context => {
				var cursor = new ILCursor(context);

				//Match 'if (knockBack > 0f && knockBackResist > 0f)' to get the address to which it jumps on failure.
				ILLabel skipKnockbackLabel = null;

				if(!cursor.TryGotoNext(
					MoveType.After,
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(NPC), nameof(NPC.knockBackResist)),
					i => i.MatchLdcR4(0f),
					i => i.Match(OpCodes.Cgt),
					i => i.Match(OpCodes.Br_S),
					i => i.Match(OpCodes.Ldc_I4_0),
					i => i.Match(OpCodes.Stloc_S),
					i => i.Match(OpCodes.Ldloc_S),
					i => i.MatchBrfalse(out skipKnockbackLabel)
				)) {
					throw new Exception($"{nameof(NPCDirectionalKnockback)}: IL Failure.");
				}

				//Match 'float num4 = knockBack * knockBackResist' to get the number of the 'num4' local.
				int totalKnockbackLocalId = 0;

				if(!cursor.TryGotoNext(
					MoveType.After,
					i => i.Match(OpCodes.Ldarg_2), //Loads the 'knockback' parameter
					i => i.Match(OpCodes.Ldarg_0), //Loads 'this'
					i => i.MatchLdfld(typeof(NPC), nameof(NPC.knockBackResist)),
					i => i.Match(OpCodes.Mul),
					i => i.MatchStloc(out totalKnockbackLocalId)
				)) {
					throw new Exception($"{nameof(NPCDirectionalKnockback)}: IL Failure.");
				}

				//Match 'int num9 = (int)num * 10;' to place code after it.
				if(!cursor.TryGotoNext(
					MoveType.After,
					i => i.Match(OpCodes.Ldloc_1),
					i => i.Match(OpCodes.Conv_I4),
					i => i.MatchLdcI4(10),
					i => i.Match(OpCodes.Mul),
					i => i.Match(OpCodes.Stloc_S)
				)) {
					throw new Exception($"{nameof(NPCDirectionalKnockback)}: IL Failure.");
				}

				cursor.Emit(OpCodes.Ldarg_0); //Load 'this'.
				cursor.Emit(OpCodes.Ldloc, totalKnockbackLocalId); //Load the local with total knockback.
				cursor.EmitDelegate<Func<NPC, float, bool>>((npc, totalKnockback) => {
					if(npc.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback) && npcKnockback.knockbackDirection.HasValue) {
						npc.velocity += npcKnockback.knockbackDirection.Value * totalKnockback;

						npcKnockback.knockbackDirection = null;

						return true;
					}

					return false;
				});
				cursor.Emit(OpCodes.Brtrue_S, skipKnockbackLabel);
			};
		}

		public void SetNextKnockbackDirection(Vector2 direction)
		{
			knockbackDirection = direction;
		}
	}
}
