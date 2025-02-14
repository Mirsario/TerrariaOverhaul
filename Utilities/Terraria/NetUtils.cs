// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.IO;
using Terraria;
using Terraria.ID;

namespace TerrariaOverhaul.Utilities.Terraria;

public static class NetUtils
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
