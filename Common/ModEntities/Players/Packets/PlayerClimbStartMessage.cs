using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players.Packets
{
	public class PlayerClimbStartMessage : NetPacket
	{
		public PlayerClimbStartMessage(Player player, Vector2 posFrom, Vector2 posTo)
		{
			Writer.TryWriteSenderPlayer(player);

			Writer.WriteVector2(posFrom);
			Writer.WriteVector2(posTo);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			if (!reader.TryReadSenderPlayer(sender, out var player)) {
				return;
			}

			var posFrom = reader.ReadVector2();
			var posTo = reader.ReadVector2();

			player.GetModPlayer<PlayerClimbing>().StartClimbing(posFrom, posTo);

			// Resend
			if (Main.netMode == NetmodeID.Server) {
				MultiplayerSystem.SendPacket(new PlayerClimbStartMessage(player, posFrom, posTo), ignoreClient: sender);
			}
		}
	}
}
