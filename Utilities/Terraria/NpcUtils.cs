// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using Terraria;

namespace TerrariaOverhaul.Utilities.Terraria;

internal static class NpcUtils
{
	public static Entity? GetTarget(this NPC npc)
	{
		if (!npc.HasValidTarget) {
			return null;
		}

		return npc.HasPlayerTarget ? Main.player[npc.target] : Main.npc[npc.target - 300];
	}

	public static NPC FindMainSegment(this NPC npc)
	{
		if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs) {
			var mainNpc = Main.npc[npc.realLife];
			if (mainNpc.active) {
				return mainNpc;
			}
		}

		return npc;
	}

	public static IEnumerable<NPC> FindSegments(this NPC npc)
	{
		int npcType = npc.type;

		for (int i = 0; i < Main.maxNPCs; i++) {
			var otherNpc = Main.npc[i];
			if (otherNpc.active && otherNpc.realLife == npcType) {
				yield return otherNpc;
			}
		}
	}

	public static NPC FindRandomSegment(this NPC npc)
	{
		int npcId = npc.whoAmI;
		int numSegments = 1;
		Span<int> segmentIds = stackalloc int[Main.maxNPCs];

		segmentIds[0] = npcId;

		for (int i = 0; i < Main.maxNPCs; i++) {
			var otherNpc = Main.npc[i];
			if (otherNpc != npc && otherNpc.active && otherNpc.realLife == npcId) {
				segmentIds[numSegments++] = i;
			}
		}

		int chosenId = segmentIds[Main.rand.Next(numSegments)];
		var chosenNpc = Main.npc[chosenId];

		return chosenNpc;
	}
}
