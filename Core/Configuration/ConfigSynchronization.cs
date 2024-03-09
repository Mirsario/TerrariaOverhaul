using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Networking;

namespace TerrariaOverhaul.Core.Configuration;

public sealed class ConfigPacket : NetPacket
{
	public ConfigPacket()
	{
		ConfigSynchronization.NetWrite(Writer);
	}

	public override void Read(BinaryReader reader, int sender)
	{
		ConfigSynchronization.NetReceive(reader, sender);
	}
}

public sealed class ConfigSynchronization : ModSystem
{
	private static bool configNeedsSynchronization;

	private static ref readonly ConfigFormat ConfigFormat => ref NbtConfig.Format;

	public override void PostUpdateEverything()
	{
		if (Main.netMode == NetmodeID.Server && configNeedsSynchronization) {
			SendConfiguration();

			configNeedsSynchronization = false;
		}
	}

	public override bool HijackSendData(int whoAmI, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
	{
		if (Main.netMode == NetmodeID.Server && msgType == MessageID.FinishedConnectingToServer && remoteClient >= 0 && remoteClient < 255) {
			SendConfiguration(toClient: remoteClient);
		}

		return false;
	}

	public static void EnqueueSynchronization()
	{
		if (Main.netMode == NetmodeID.Server) {
			configNeedsSynchronization = true;
		}
	}

	public static void SendConfiguration(int toClient = -1, int ignoreClient = -1)
	{
		if (Main.netMode != NetmodeID.Server) {
			return;
		}

		DebugSystem.Logger.Debug($"Sending configuration to {(toClient >= 0 ? $"client {toClient}" : $"all clients{(ignoreClient >= 0 ? $", except {ignoreClient}" : null)}")}.");

		MultiplayerSystem.SendPacket(new ConfigPacket(), toClient: toClient, ignoreClient: ignoreClient);
	}

	internal static void NetWrite(BinaryWriter writer)
	{
		ConfigIO.ExportConfig(out var export, fromLocal: true);
		ConfigFormat.WriteConfig(writer.BaseStream, in export);
	}

	internal static void NetReceive(BinaryReader reader, int sender)
	{
		var ioResult = ConfigFormat.ReadConfig(reader.BaseStream, out var export);

		if (Main.netMode != NetmodeID.MultiplayerClient || (sender is >= 0 and < Main.maxPlayers)) {
			DebugSystem.Logger.Warn("Received configuration from a client, this shouldn't happen!");
			return;
		}

		if (ioResult == ConfigIO.Result.Success) {
			DebugSystem.Logger.Debug($"Received server configuration.");
		} else {
			DebugSystem.Logger.Warn($"Received faulty configuration from the server: '{ioResult}'.");
		}

		if (!ioResult.HasFlag(ConfigIO.Result.ErrorFlag)) {
			ConfigIO.ImportConfig(in export, intoLocal: false);
		}
	}
}
