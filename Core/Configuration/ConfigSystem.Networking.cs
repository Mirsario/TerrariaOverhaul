using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

#pragma warning disable CA1822 // Mark members as static

namespace TerrariaOverhaul.Core.Configuration;

public sealed partial class ConfigSystem : ModSystem
{
	private static bool configNeedsSynchronization;

	private void InitializeNetworking()
	{
		configNeedsSynchronization = false;
	}

	public override void PostUpdateWorld()
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

	internal static void NetWriteConfiguration(BinaryWriter writer)
	{
		// Configuration packets consist of deflate-compressed serialized JSONs with no formatting
		var jObject = new JObject();
		var configEntries = entriesByName.Values;

		foreach (var entry in configEntries) {
			if (entry.Side != ConfigSide.Both || entry.LocalValue == null) {
				continue;
			}

			if (!jObject.TryGetValue(entry.Category, out var categoryToken)) {
				jObject[entry.Category] = categoryToken = new JObject();
			}

			categoryToken[entry.Name] = JToken.FromObject(entry.LocalValue);
		}

		string jsonText = jObject.ToString(Newtonsoft.Json.Formatting.None);
		byte[] data = Encoding.UTF8.GetBytes(jsonText);
		byte[] compressedData = CompressionUtils.DeflateCompress(data);

		writer.Write7BitEncodedInt(compressedData.Length);
		writer.Write(compressedData);
	}

	internal static void NetReadConfiguration(BinaryReader reader)
	{
		int compressedLength = reader.Read7BitEncodedInt();
		byte[] compressedData = reader.ReadBytes(compressedLength);
		byte[] data = CompressionUtils.DeflateDecompress(compressedData);

		string jsonText = Encoding.UTF8.GetString(data);
		var jObject = JObject.Parse(jsonText);

		foreach (var categoryPair in jObject) {
			if (categoryPair.Value is not JObject categoryJson || !categoriesByName.TryGetValue(categoryPair.Key, out var category)) {
				continue;
			}

			foreach (var entryPair in categoryJson) {
				if (!category.EntriesByName.TryGetValue(entryPair.Key, out var entry)) {
					continue;
				}

				object? value = entryPair.Value?.ToObject(entry.ValueType);

				if (value != null) {
					entry.RemoteValue = value;
				}
			}
		}

		DebugSystem.Logger.Debug("Received server configuration.");
	}
}
