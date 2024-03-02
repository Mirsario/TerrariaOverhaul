using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Dodgerolls;

public sealed class PlayerDodgerollPacket : NetPacket
{
	public PlayerDodgerollPacket(Player player)
	{
		var playerDodgerolls = player.GetModPlayer<PlayerDodgerolls>();

		Writer.TryWriteSenderPlayer(player);

		Writer.Write((sbyte)playerDodgerolls.WantedDirection);
		Writer.WriteVector2(player.velocity);
	}

	public override void Read(BinaryReader reader, int sender)
	{
		if (!reader.TryReadSenderPlayer(sender, out var player)) {
			return;
		}

		var playerDodgerolls = player.GetModPlayer<PlayerDodgerolls>();

		playerDodgerolls.ForceDodgeroll = true;
		playerDodgerolls.WantedDirection = (Direction1D)reader.ReadSByte();

		player.velocity = reader.ReadVector2();

		// Resend
		if (Main.netMode == NetmodeID.Server) {
			MultiplayerSystem.SendPacket(new PlayerDodgerollPacket(player), ignoreClient: sender);
		}
	}
}
