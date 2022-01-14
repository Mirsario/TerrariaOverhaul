using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Systems.Networking;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players.Packets
{
	public class PlayerDodgerollPacket : NetPacket
	{
		public PlayerDodgerollPacket(Player player)
		{
			var playerDodgerolls = player.GetModPlayer<PlayerDodgerolls>();

			Writer.TryWriteSenderPlayer(player);

			Writer.Write(playerDodgerolls.wantedDodgerollDir);
			Writer.WriteVector2(player.velocity);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			if (!reader.TryReadSenderPlayer(sender, out var player)) {
				return;
			}

			var playerDodgerolls = player.GetModPlayer<PlayerDodgerolls>();

			playerDodgerolls.forceDodgeroll = true;
			playerDodgerolls.wantedDodgerollDir = reader.ReadSByte();

			player.velocity = reader.ReadVector2();

			// Resend
			if (Main.netMode == NetmodeID.Server) {
				MultiplayerSystem.SendPacket(new PlayerDodgerollPacket(player), ignoreClient: sender);
			}
		}
	}
}
