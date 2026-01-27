// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Charging;

internal sealed class PowerAttackStartPacket : NetPacket
{
	public PowerAttackStartPacket(Player player, int chargeLength)
	{
		NetUtils.TryWriteSenderPlayer(Writer, player);

		Writer.Write7BitEncodedInt(player.selectedItem);
		Writer.Write7BitEncodedInt(chargeLength);
	}

	public override void Read(BinaryReader reader, int sender)
	{
		if (!reader.TryReadSenderPlayer(sender, out var player)) {
			return;
		}

		int selectedItem = reader.Read7BitEncodedInt();
		int chargeLength = reader.Read7BitEncodedInt();

		if (player.selectedItem != selectedItem) {
			return;
		}

		if (player.HeldItem is not Item item || !item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks)) {
			return;
		}

		// Resend happens in this method automatically
		powerAttacks.StartPowerAttack(item, player, (uint)chargeLength);

		// Resend
		if (Main.netMode == NetmodeID.Server) {
			MultiplayerSystem.SendPacket(new PowerAttackStartPacket(player, chargeLength), ignoreClient: sender);
		}
	}
}
