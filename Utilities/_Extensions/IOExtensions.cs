using System.IO;
using Terraria;
using Terraria.ID;

namespace TerrariaOverhaul.Utilities
{
	public static class IOExtensions
	{
		public static void TryWriteSenderPlayer(this BinaryWriter writer, Player player)
		{
			if (Main.netMode == NetmodeID.Server) {
				writer.Write((byte)player.whoAmI);
			}
		}

		public static bool TryReadSenderPlayer(this BinaryReader reader, int sender, out Player player)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				sender = reader.ReadByte();
			}

			player = Main.player[sender];

			return player != null && player.active;
		}
	}
}
