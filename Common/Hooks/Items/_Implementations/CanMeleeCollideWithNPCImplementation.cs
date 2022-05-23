using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Exceptions;
using Hook = TerrariaOverhaul.Common.Hooks.Items.ICanMeleeCollideWithNPC;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	internal sealed class CanMeleeCollideWithNPCImplementation : GlobalItem
	{
		public override void Load()
		{
			IL.Terraria.Player.ItemCheck_MeleeHitNPCs += context => {
				var cursor = new ILCursor(context);

				// This is a pretty bad expression
				int itemRectangleArgId = 0;
				int npcRectangleLocalId = 0;

				cursor.GotoNext(
					MoveType.Before,
					i => i.MatchLdarga(out itemRectangleArgId),
					i => i.MatchLdloc(out npcRectangleLocalId),
					i => i.MatchCall(typeof(Rectangle), nameof(Rectangle.Intersects)),
					i => i.MatchBrfalse(out _)
				);

				cursor.RemoveRange(3);

				cursor.Emit(OpCodes.Ldarg_1); // Load 'item' argument.
				cursor.Emit(OpCodes.Ldarg_0); // Load 'this' (player) argument.
				cursor.Emit(OpCodes.Ldloc_0); // Load the id of the npc. (!) We're assuming that it's local 0. This sucks.
				cursor.Emit(OpCodes.Ldarg, itemRectangleArgId); // Load 'itemRectangle' for the fallback.
				cursor.Emit(OpCodes.Ldloc, npcRectangleLocalId); // Load 'value' (npc rectangle) for the fallback.
				cursor.EmitDelegate<Func<Item, Player, int, Rectangle, Rectangle, bool>>((item, player, npcId, itemRectangle, npcRectangle) => {
					var npc = Main.npc[npcId];

					return Hook.Invoke(item, player, npc) ?? itemRectangle.Intersects(npcRectangle);
				});
			};
		}
	}
}
