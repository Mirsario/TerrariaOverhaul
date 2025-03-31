// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.IO;
using System.IO.Compression;

namespace TerrariaOverhaul.Utilities;

internal static class CompressionUtils
{
	public static byte[] DeflateCompress(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Optimal)
	{
		using var outputStream = new MemoryStream();
		using var deflateStream = new DeflateStream(outputStream, compressionLevel, leaveOpen: true);

		deflateStream.Write(data);
		deflateStream.Flush();

		return outputStream.ToArray();
	}

	public static byte[] DeflateDecompress(byte[] data)
	{
		using var inputStream = new MemoryStream(data);
		using var outputStream = new MemoryStream();
		using var deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress, leaveOpen: true);

		deflateStream.CopyTo(outputStream);

		return outputStream.ToArray();
	}
}
