using System;
using System.Linq;
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
		IL_Player.ItemCheck_MeleeHitNPCs += context => {
			var il = new ILCursor(context);
			bool debugAssembly = OverhaulMod.TMLAssembly.IsDebugAssembly();

			// Match:
			// for (int i = 0; i < 200; i++)
			// To get labels and 'i'.

			int npcIdLocalId = 0;
			ILLabel? continueLabel = null;

			il.GotoNext(
				i => i.MatchLdcI4(0),
				i => i.MatchStloc(out npcIdLocalId),
				i => i.MatchBr(out continueLabel)
			);

			// Match:
			// NPC npc = Main.npc[i];
			// To get the local.

			int npcLocalId = 0;

			il.GotoNext(
				i => i.MatchLdsfld(typeof(Main), nameof(Main.npc)),
				i => i.MatchLdloc(npcIdLocalId),
				i => i.MatchLdelemRef(),
				i => i.MatchStloc(out npcLocalId)
			);

			/*
			ILLabel? checkSkipLabel = null;
			ILLabel? tempLabel = null;

			il.HijackIncomingLabels();

			// Create local var
			int callResultLocalId = il.AddLocalVariable(typeof(bool?));

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
			*/
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
