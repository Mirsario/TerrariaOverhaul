using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemMeleeCooldownReplacement : ItemComponent
{
	public override void Load()
	{
		// Disable attackCD for melee whenever this component is present on the held item and enabled.
		IL.Terraria.Player.ItemCheck_MeleeHitNPCs += context => {
			var il = new ILCursor(context);
			bool debugAssembly = typeof(Main).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration?.Contains("Debug") == true;

			ILLabel? continueLabel = null;
			ILLabel? checkSkipLabel = null;
			ILLabel? tempLabel = null;
			int npcIdLocalId = 0;

			// if (!Main.npc[i].active || Main.npc[i].immune[whoAmI] != 0 || attackCD != 0)
			il.GotoNext(
				MoveType.Before,
				new Func<Instruction, bool>[] {
				//il.CreateDebugInstructionPredicates(new Expression<Func<Instruction, bool>>?[] {
					// ...
					i => i.MatchBr(out continueLabel),

					// !Main.npc[i].active
					debugAssembly ? i => i.MatchNop() : null!,
					i => i.MatchLdsfld(typeof(Main), nameof(Main.npc)),
					i => i.MatchLdloc(out npcIdLocalId),
					i => i.MatchLdelemRef(),
					i => i.MatchLdfld(typeof(Entity), nameof(Entity.active)),
					i => i.MatchBrfalse(out checkSkipLabel),

					// Main.npc[i].immune[whoAmI] != 0
					i => i.MatchLdsfld(typeof(Main), nameof(Main.npc)),
					i => i.MatchLdloc(npcIdLocalId),
					i => i.MatchLdelemRef(),
					i => i.MatchLdfld(typeof(NPC), nameof(NPC.immune)),
					i => i.MatchLdarg(0),
					i => i.MatchLdfld(typeof(Entity), nameof(Entity.whoAmI)),
					i => i.MatchLdelemI4(),
					//i => i.MatchBrtrue(checkSkipLabel),
					i => i.MatchBrtrue(out tempLabel) && tempLabel.Target!.Offset == checkSkipLabel!.Target!.Offset,

					// attackCD != 0
					i => i.MatchLdarg(0),
					i => i.MatchLdfld(typeof(Player), nameof(Player.attackCD)),
					// ...
				//})
				}
				.Where(p => p != null).ToArray()
			);

			il.HijackIncomingLabels();

			// Create local var
			var nullableBoolType = context.Import(typeof(bool?));
			int callResultLocalId = context.Body.Variables.Count;
			
			il.Body.Variables.Add(new VariableDefinition(nullableBoolType));

			// Load 'this' (player)
			il.Emit(OpCodes.Ldarg_0);
			// Load NPC
			//il.Emit(OpCodes.Ldsfld, context.Import(typeof(Main).GetField(nameof(Main.npc))));
			il.Emit(OpCodes.Ldloc, npcIdLocalId);
			//il.Emit(OpCodes.Ldelem_Ref);
			// Invoke delegate & store the result
			il.EmitDelegate(CanAttackNPC);
			il.Emit(OpCodes.Stloc, callResultLocalId);

			// If the result is true - Skip over original checks if true is returned
			il.Emit(OpCodes.Ldloc, callResultLocalId);
			il.Emit(OpCodes.Ldc_I4_1);
			il.Emit(OpCodes.Beq, checkSkipLabel!);

			// If the result is false - 'continue;' in the loop.
			il.Emit(OpCodes.Ldloc, callResultLocalId);
			il.Emit(OpCodes.Ldc_I4_0);
			il.Emit(OpCodes.Beq, continueLabel!);

			// On null - the original checks get to run.
		};
	}

	private static bool? CanAttackNPC(Player player, int npcId)
	{
		var npc = Main.npc[npcId];

		if (player.HeldItem?.IsAir == false && player.HeldItem.TryGetGlobalItem(out ItemMeleeCooldownReplacement replacement) && replacement.Enabled) {
			return true;
		}

		return false;
	}
}
