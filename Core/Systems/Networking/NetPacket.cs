using System;
using System.IO;

namespace TerrariaOverhaul.Core.Systems.Networking
{
	public abstract class NetPacket
	{
		public int Id { get; internal set; }

		private readonly Action<BinaryWriter> Writer;

		protected NetPacket(Action<BinaryWriter> writer)
		{
			Id = MultiplayerSystem.GetPacket(GetType()).Id;
			Writer = writer ?? throw new ArgumentNullException(nameof(writer));
		}

		public abstract void Read(BinaryReader reader,int sender);

		public void Write(BinaryWriter writer) => Writer(writer);
	}
}
