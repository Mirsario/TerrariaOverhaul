using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Systems.Networking;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players.Packets
{
	public class PlayerMousePositionPacket : NetPacket
	{
		public PlayerMousePositionPacket(Player player) : base(w => {
			var modPlayer = player.GetModPlayer<PlayerDirectioning>();

			w.TryWriteSenderPlayer(player);

			w.WriteVector2(modPlayer.mouseWorld);
		})
		{ }

		public override void Read(BinaryReader reader, int sender)
		{
			if(!reader.TryReadSenderPlayer(sender, out var player)) {
				return;
			}

			var modPlayer = player.GetModPlayer<PlayerDirectioning>();

			modPlayer.mouseWorld = reader.ReadVector2();

			//Resend
			if(Main.netMode == NetmodeID.Server) {
				MultiplayerSystem.SendPacket(new PlayerMousePositionPacket(player), ignoreClient: sender);
			}
		}
	}
}
