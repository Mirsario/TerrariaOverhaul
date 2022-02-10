using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players.Packets
{
	public class PlayerMousePositionPacket : NetPacket
	{
		public PlayerMousePositionPacket(Player player)
		{
			var modPlayer = player.GetModPlayer<PlayerDirectioning>();

			Writer.TryWriteSenderPlayer(player);

			Writer.WriteVector2(modPlayer.MouseWorld);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			if (!reader.TryReadSenderPlayer(sender, out var player)) {
				return;
			}

			var modPlayer = player.GetModPlayer<PlayerDirectioning>();

			modPlayer.MouseWorld = reader.ReadVector2();

			// Resend
			if (Main.netMode == NetmodeID.Server) {
				MultiplayerSystem.SendPacket(new PlayerMousePositionPacket(player), ignoreClient: sender);
			}
		}
	}
}
