using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaOverhaul.Common.Systems.Lighting
{
	public class Surface<T> : IDisposable where T : unmanaged
	{
		public T[] Data { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public ref T this[int x, int y] => ref Data[Width * y + x];

		public Surface(int width, int height)
		{
			Data = new T[width * height];
			Width = width;
			Height = height;
		}

		public void Dispose()
		{
			Data = null;
			Width = -1;
			Height = -1;
		}
	}
}
