using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Terraria.ModLoader.IO;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;
using IOResult = TerrariaOverhaul.Core.Configuration.ConfigIO.Result;

namespace TerrariaOverhaul.Core.Configuration;

public static class NbtConfig
{
	public static readonly ConfigFormat Format;

	static NbtConfig()
	{
		Format.ReadConfig = ReadConfig;
		Format.WriteConfig = WriteConfig;
		Format.Extension = ".nbt";
	}

	public static IOResult WriteConfig(Stream stream, in ConfigExport configExport)
	{
		var tag = new TagCompound();

		if (configExport.ModVersion is Version modVersion) {
			tag["Meta"] = new TagCompound {
				["ModVersion"] = modVersion.ToString(),
			};
		}

		var entriesByName = ConfigSystem.EntriesByName;
		var valuesByEntry = configExport.ValuesByEntry;

		foreach (var entry in entriesByName.Values.OrderBy(e => $"{e.Category}.{e.Name}")) {
			if (!valuesByEntry.TryGetValue(entry.Name, out object? value)) {
				continue;
			}

			if (!tag.TryGet<TagCompound>(entry.Category, out var categoryToken)) {
				tag[entry.Category] = categoryToken = new TagCompound();
			}

			categoryToken[entry.Name] = value;
		}

		using var binaryWriter = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
		using var memoryStream = new MemoryStream();
		TagIO.ToStream(tag, memoryStream, compress: false);
		memoryStream.Flush();
		byte[] data = CompressionUtils.DeflateCompress(memoryStream.ToArray());

		binaryWriter.Write7BitEncodedInt(data.Length);
		stream.Write(data);

		return IOResult.Success;
	}

	public static IOResult ReadConfig(Stream stream, out ConfigExport configExport)
	{
		var tagGetMethod = typeof(TagCompound).GetMethod("Get", BindingFlags.Public | BindingFlags.Instance)!;

		using var binaryReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
		int length = binaryReader.Read7BitEncodedInt();
		byte[] compressedData = binaryReader.ReadBytes(length);
		using var memoryStream = new MemoryStream(CompressionUtils.DeflateDecompress(compressedData));

		try {
			var tag = TagIO.FromStream(memoryStream, compressed: false);

			if (tag == null) {
				configExport = default;
				return IOResult.FileBroken;
			}

			configExport.ModVersion = null;

			if (tag.TryGet<TagCompound>("Meta", out var metaToken) && metaToken is TagCompound metaObject) {
				if (metaObject.TryGet("ModVersion", out string modVersionString) && modVersionString != null) {
					if (Version.TryParse(modVersionString, out var version)) {
						configExport.ModVersion = version;
					}
				}
			}

			bool hadErrors = false;
			var categoriesByName = ConfigSystem.CategoriesByName;

			configExport.ValuesByEntry = new();

			foreach (var categoryPair in tag) {
				if (categoryPair.Value is not TagCompound categoryTag) {
					continue;
				}

				if (!categoriesByName.TryGetValue(categoryPair.Key, out var category)) {
					continue;
				}

				foreach (var entryPair in categoryTag) {
					if (!category.EntriesByName.TryGetValue(entryPair.Key, out var entry)) {
						continue;
					}

					object? value = null;

					if (entryPair.Value != null) {
						value = tagGetMethod
							.MakeGenericMethod(entry.ValueType)
							.Invoke(categoryTag, new object[] { entryPair.Key });
					}

					if (value != null) {
						configExport.ValuesByEntry[entry.Name] = value;
					} else {
						hadErrors = true;
					}
				}
			}

			return hadErrors ? IOResult.HadErrors : IOResult.Success;
		}
		catch {
			configExport = default;
			return IOResult.FileBroken;
		}
	}
}
