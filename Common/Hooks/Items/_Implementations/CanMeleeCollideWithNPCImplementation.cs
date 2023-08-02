using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanMeleeCollideWithNPC;

namespace TerrariaOverhaul.Common.Hooks.Items;

internal sealed class CanMeleeCollideWithNPCImplementation : GlobalItem
{
	public override void Load()
	{
		IL_Player.ProcessHitAgainstNPC += context => {
			var cursor = new ILCursor(context);

			// Match:
			// NPC npc = Main.npc[npcIndex];
			// To get the NPC local.

			int npcLocalId = 0;

			cursor.GotoNext(
				MoveType.Before,
				i => i.MatchLdsfld(typeof(Main), nameof(Main.npc)),
				i => i.MatchLdarg(out _),
				i => i.MatchLdelemRef(),
				i => i.MatchStloc(out npcLocalId)
			);

			// Match:
			// bool flag2 = ((Rectangle)(ref itemRectangle)).Intersects(val);
			// For relocation.

			int itemRectangleArgId = 0;
			int checkResultLocalId = 0;

			cursor.GotoNext(
				MoveType.Before,
				i => i.MatchLdarga(out itemRectangleArgId),
				i => i.MatchLdloc(out _),
				i => i.MatchCall(typeof(Rectangle), nameof(Rectangle.Intersects)),
				i => i.MatchStloc(out checkResultLocalId)
			);

			// Insert our changes

			static bool TryGetOverride(Item item, Player player, NPC npc, in Rectangle itemRectangle, out bool result)
			{
				bool? hookResult = Hook.Invoke(item, player, npc, itemRectangle);

				result = hookResult ?? false;

				return hookResult.HasValue;
			}

			cursor.HijackIncomingLabels(); // Just in case.

			cursor.Emit(OpCodes.Ldarg_1); // Load 'item' argument.
			cursor.Emit(OpCodes.Ldarg_0); // Load 'this' (player) argument.
			cursor.Emit(OpCodes.Ldloc, npcLocalId); // Load the 'npc' local.
			cursor.Emit(OpCodes.Ldarga, itemRectangleArgId); // Load the address of the item rectangle argument.
			cursor.Emit(OpCodes.Ldloca, checkResultLocalId); // Load the address of the check result local.
			cursor.EmitDelegate(TryGetOverride);

			// If the above method returns true - jump over the vanilla code, onto a yet-to-be-written label.

			var skipRectangleIntersectionLabel = cursor.DefineLabel();

			cursor.Emit(OpCodes.Brtrue, skipRectangleIntersectionLabel);

			cursor.Index += 4; // Not that great!

			cursor.MarkLabel(skipRectangleIntersectionLabel);
		};
	}
}
