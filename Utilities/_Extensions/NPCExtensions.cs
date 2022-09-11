using System;
using System.Collections.Generic;
using Terraria;

namespace TerrariaOverhaul.Utilities;

public static class NpcExtensions
{
	public static Entity? GetTarget(this NPC npc)
	{
		if (!npc.HasValidTarget) {
			return null;
		}

		return npc.HasPlayerTarget ? Main.player[npc.target] : Main.npc[npc.target - 300];
	}

	public static NPC GetMainSegment(this NPC npc)
	{
		if (npc.realLife >= 0 && npc.realLife < Main.maxNPCs) {
			var mainNpc = Main.npc[npc.realLife];

			if (mainNpc.active) {
				return mainNpc;
			}
		}

		return npc;
	}

	public static IEnumerable<NPC> EnumerateSegments(this NPC npc)
	{
		int npcType = npc.type;

		for (int i = 0; i < Main.maxNPCs; i++) {
			var otherNpc = Main.npc[i];

			if (otherNpc.active && otherNpc.realLife == npcType) {
				yield return otherNpc;
			}
		}
	}

	public static NPC GetRandomSegment(this NPC npc)
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
