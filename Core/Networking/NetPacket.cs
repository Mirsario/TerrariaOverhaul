// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.IO;

namespace TerrariaOverhaul.Core.Networking;

public abstract class NetPacket : IDisposable
{
	public int Id { get; internal set; }

	protected BinaryWriter Writer { get; private set; }

	private MemoryStream stream;

	protected NetPacket()
	{
		Id = MultiplayerSystem.GetPacket(GetType()).Id;
		Writer = new BinaryWriter(stream = new MemoryStream());
	}

	public abstract void Read(BinaryReader reader, int sender);

	public void WriteAndDispose(BinaryWriter writer)
	{
		writer.Write(stream.ToArray());

		Dispose();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);

		Writer?.Dispose();
		stream?.Dispose();

		Writer = null!;
		stream = null!;
	}
}
