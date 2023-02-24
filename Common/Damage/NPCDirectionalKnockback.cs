using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Movement;

namespace TerrariaOverhaul.Common.Damage;

public sealed class NPCDirectionalKnockback : GlobalNPC
{
	private Vector2? knockbackDirection;

	public override bool InstancePerEntity => true;

	public override void Load()
	{
		IL_NPC.StrikeNPC += context => {
			var cursor = new ILCursor(context);

			// Match 'if (knockBack > 0f && knockBackResist > 0f)' to get the address to which it jumps on failure.
			ILLabel? skipKnockbackLabel = null;

			if (!cursor.TryGotoNext(
				MoveType.After,
				i => i.Match(OpCodes.Ldarg_2), // 'knockBack' parameter.
				i => i.MatchLdcR4(0f),
				i => i.MatchBleUn(out skipKnockbackLabel)
			)) {
				throw new Exception($"{nameof(NPCDirectionalKnockback)}: IL_NPC.StrikeNPC Failure at match 1.");
			}

			// Match 'float num4 = knockBack * knockBackResist' to get the number of the 'num4' local.
			int totalKnockbackLocalId = 0;

			if (!cursor.TryGotoNext(
				MoveType.After,
				i => i.Match(OpCodes.Ldarg_2), // Loads the 'knockback' parameter
				i => i.Match(OpCodes.Ldarg_0), // Loads 'this'
				i => i.MatchLdfld(typeof(NPC), nameof(NPC.knockBackResist)),
				i => i.Match(OpCodes.Mul),
				i => i.MatchStloc(out totalKnockbackLocalId)
			)) {
				throw new Exception($"{nameof(NPCDirectionalKnockback)}: IL_NPC.StrikeNPC Failure at match 2.");
			}

			// Match 'int num9 = (int)num * 10;' to place code after it.
			if (!cursor.TryGotoNext(
				MoveType.After,
				i => i.Match(OpCodes.Ldloc_1),
				i => i.Match(OpCodes.Conv_I4),
				i => i.MatchLdcI4(10),
				i => i.Match(OpCodes.Mul),
				i => i.Match(OpCodes.Stloc_S)
			)) {
				throw new Exception($"{nameof(NPCDirectionalKnockback)}: IL_NPC.StrikeNPC Failure at match 3.");
			}

			cursor.Emit(OpCodes.Ldarg_0); // Load 'this'.
			cursor.Emit(OpCodes.Ldloc, totalKnockbackLocalId); // Load the local with total knockback.
			cursor.EmitDelegate(ApplyKnockbackOverride);
			cursor.Emit(OpCodes.Brtrue_S, skipKnockbackLabel!);
		};
	}

	public void SetNextKnockbackDirection(Vector2 direction)
	{
		knockbackDirection = direction;
	}

	private static bool ApplyKnockbackOverride(NPC npc, float totalKnockback)
	{
		if (npc.TryGetGlobalNPC(out NPCDirectionalKnockback npcKnockback) && npcKnockback.knockbackDirection.HasValue) {
			Vector2 knockback = npcKnockback.knockbackDirection.Value * totalKnockback;
			Vector2 maxVelocity = Vector2.UnitX * MathF.Abs(totalKnockback);

			VelocityUtils.AddLimitedVelocity(npc, knockback, maxVelocity);

			npcKnockback.knockbackDirection = null;

			return true;
		}

		return false;
	}
}
