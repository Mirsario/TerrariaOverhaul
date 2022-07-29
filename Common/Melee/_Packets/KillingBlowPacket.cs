using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class KillingBlowPacket : NetPacket
{
	public KillingBlowPacket(Player player, NPC victimNPC)
	{
		Writer.TryWriteSenderPlayer(player);

		Writer.Write((short)victimNPC.whoAmI);
	}

	public override void Read(BinaryReader reader, int sender)
	{
		if (!reader.TryReadSenderPlayer(sender, out var player)) {
			return;
		}

		int npcId = reader.ReadInt16();

		if (npcId >= 0 && npcId < Main.maxNPCs && Main.npc[npcId] is NPC { active: true } npc) {
			ItemKillingBlows.EnqueueNPCForKillingBlowHit(npcId);
			
			// Resend
			if (Main.netMode == NetmodeID.Server) {
				MultiplayerSystem.SendPacket(new KillingBlowPacket(player, npc), ignoreClient: sender);
			}
		}
	}
}
