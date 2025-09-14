// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Melee;

internal sealed class KillingBlowEffectsPacket : NetPacket
{
	public KillingBlowEffectsPacket(Player player, Vector2Int worldPosition)
	{
		NetUtils.TryWriteSenderPlayer(Writer, player);
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
