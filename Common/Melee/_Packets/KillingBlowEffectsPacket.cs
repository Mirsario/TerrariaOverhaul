using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class KillingBlowEffectsPacket : NetPacket
{
	public KillingBlowEffectsPacket(Player player, Vector2Int worldPosition)
	{
		Writer.TryWriteSenderPlayer(player);
		Writer.WriteVector2Int(worldPosition);
	}

	public override void Read(BinaryReader reader, int sender)
	{
		if (!reader.TryReadSenderPlayer(sender, out var player)) {
			return;
		}

		var worldPosition = reader.ReadVector2Int();

		ItemKillingBlows.CreateEffects(worldPosition);
		
		// Resend
		if (Main.netMode == NetmodeID.Server) {
			MultiplayerSystem.SendPacket(new KillingBlowEffectsPacket(player, worldPosition), ignoreClient: sender);
		}
	}
}
