using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Damage;

public sealed class NPCDirectionalKnockback : GlobalNPC
{
	private Vector2? knockbackDirection;

	public override bool InstancePerEntity => true;

	public override void Load()
	{
		IL_NPC.StrikeNPC_HitInfo_bool_bool += context => {
			var cursor = new ILCursor(context);
			bool debugAssembly = OverhaulMod.TMLAssembly.IsDebugAssembly();

			// Match 'if (hit.Knockback > 0f)' to get the address to which it jumps on failure.
			ILLabel? skipKnockbackLabel = null;

			if (!debugAssembly) {
				cursor.GotoNext(
					MoveType.After,
					i => i.MatchLdfld(typeof(NPC.HitInfo), nameof(NPC.HitInfo.Knockback)),
					i => i.MatchLdcR4(0f),
					i => i.MatchBleUn(out skipKnockbackLabel)
				);
			} else {
				cursor.GotoNext(
					MoveType.After,
					i => i.MatchLdarg(1),
					i => i.MatchLdfld(typeof(NPC.HitInfo), nameof(NPC.HitInfo.Knockback)),
					i => i.MatchLdcR4(0f),
					i => i.MatchCgt(),
					i => i.MatchStloc(out _),
					i => i.MatchLdloc(out _),
					i => i.MatchBrfalse(out skipKnockbackLabel)
				);
			}

			// Match 'float num3 = hit.Knockback;' to get the number of the 'num3' local.
			int totalKnockbackLocalId = 0;

			cursor.GotoNext(
				MoveType.After,
				i => i.Match(OpCodes.Ldarg_1), // Loads 'HitInfo hit'
				i => i.MatchLdfld(typeof(NPC.HitInfo), nameof(NPC.HitInfo.Knockback)),
				i => i.MatchStloc(out totalKnockbackLocalId)
			);

			// Match 'if (Main.expertMode)' to place code before it.
			cursor.GotoNext(
				MoveType.Before,
				i => i.MatchCall(typeof(Main), $"get_{nameof(Main.expertMode)}")
			);

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
