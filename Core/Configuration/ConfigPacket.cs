using System.IO;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Core.Networking;

namespace TerrariaOverhaul.Core.Configuration;

public class ConfigPacket : NetPacket
{
	public ConfigPacket()
	{
		ConfigSystem.NetWriteConfiguration(Writer);
	}

	public override void Read(BinaryReader reader, int sender)
	{
		if (Main.netMode != NetmodeID.MultiplayerClient) {
			return;
		}

		ConfigSystem.NetReadConfiguration(reader);
	}
}
