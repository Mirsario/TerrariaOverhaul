using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Ionic.Zlib;
using Microsoft.Xna.Framework.Media;
using ReLogic.Content;
using ReLogic.Content.Readers;
using ReLogic.Utilities;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using static Theorafile;

namespace TerrariaOverhaul.Core.VideoPlayback;

[Autoload(false)]
public sealed class OgvReader : IAssetReader, ILoadable
{
	private const BindingFlags ReflectionFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	public static readonly string Extension = ".ogv";

	private static readonly Dictionary<IntPtr, UnmanagedMemoryStream> memoryStreams = new();
	// Stores delegates in heap so that they don't get eaten by the GC.
	private static readonly tf_callbacks callbacks = new() {
		read_func = ReadFunction,
		seek_func = SeekFunction,
		close_func = CloseFunction,
	};
	// Reflection
	private static readonly Type videoType = typeof(Video);
	private static readonly FieldInfo videoTheora = videoType.GetField("theora", ReflectionFlags)!;
	private static readonly FieldInfo videoYWidth = videoType.GetField("yWidth", ReflectionFlags)!;
	private static readonly FieldInfo videoYHeight = videoType.GetField("yHeight", ReflectionFlags)!;
	private static readonly FieldInfo videoUvWidth = videoType.GetField("uvWidth", ReflectionFlags)!;
	private static readonly FieldInfo videoUvHeight = videoType.GetField("uvHeight", ReflectionFlags)!;
	private static readonly FieldInfo videoFps = videoType.GetField("fps", ReflectionFlags)!;
	private static readonly FieldInfo videoNeedsDurationHack = videoType.GetField("needsDurationHack", ReflectionFlags)!;
	private static readonly PropertyInfo videoDuration = videoType.GetProperty(nameof(Video.Duration), ReflectionFlags)!;
	private static readonly PropertyInfo videoGraphicsDevice = videoType.GetProperty("GraphicsDevice", ReflectionFlags)!;

	public async ValueTask<T> FromStream<T>(Stream stream, MainThreadCreationContext mainThreadCtx) where T : class
	{
		if (typeof(T) != videoType) {
			throw AssetLoadException.FromInvalidReader<OgvReader, T>();
		}

		await mainThreadCtx;

		var result = CreateVideo(stream);

		return (result as T)!;
	}

	private unsafe Video CreateVideo(Stream stream)
	{
		// This is created only to get a length without accessing stream.Length,
		// because 'stream' may be 'DeflateStream', and that doesn't implement it.
		// Could be avoided.
		using var memoryStream = new MemoryStream();

		stream.CopyTo(memoryStream);

		int numBytes = (int)memoryStream.Position;
		var dataPtr = Marshal.AllocHGlobal(numBytes);
		var unmanagedStream = new UnmanagedMemoryStream((byte*)dataPtr, numBytes, numBytes, FileAccess.ReadWrite);

		memoryStream.Seek(0, SeekOrigin.Begin);
		memoryStream.CopyTo(unmanagedStream, numBytes);
		unmanagedStream.Seek(0L, SeekOrigin.Begin);

		// Keep track of streams and the data pointers.
		memoryStreams[dataPtr] = unmanagedStream;

		// Video's constructors are useless - they're internal, and take an OS file path rather than a data pointer.
		// Here we assemble the Video instance completely by ourselves.
		// Good thing that we no longer have to account for different frameworks being used.
		// - Mirsario.
		var result = (Video)FormatterServices.GetUninitializedObject(videoType);

		int openResult = tf_open_callbacks(dataPtr, out var theoraPtr, callbacks);

		if (openResult != 0) {
			throw new InvalidOperationException($"Theorafile returned code '{openResult}' when trying to load data.");
		}

		tf_videoinfo(theoraPtr, out int yWidth, out int yHeight, out double fps, out var fmt);

		int uvWidth;
		int uvHeight;

		if (fmt == th_pixel_fmt.TH_PF_420) {
			uvWidth = yWidth / 2;
			uvHeight = yHeight / 2;
		} else if (fmt == th_pixel_fmt.TH_PF_422) {
			uvWidth = yWidth / 2;
			uvHeight = yHeight;
		} else if (fmt == th_pixel_fmt.TH_PF_444) {
			uvWidth = yWidth;
			uvHeight = yHeight;
		} else {
			throw new NotSupportedException("Unrecognized YUV format!");
		}

		videoGraphicsDevice.SetValue(result, Main.graphics.GraphicsDevice);
		videoTheora.SetValue(result, theoraPtr);
		videoYWidth.SetValue(result, yWidth);
		videoYHeight.SetValue(result, yHeight);
		videoUvWidth.SetValue(result, uvWidth);
		videoUvHeight.SetValue(result, uvHeight);
		videoFps.SetValue(result, fps);

		videoDuration.SetValue(result, TimeSpan.MaxValue);
		videoNeedsDurationHack.SetValue(result, true);

		return result;
	}

	void ILoadable.Load(Mod mod)
	{
		var assetReaderCollection = Main.instance.Services.Get<AssetReaderCollection>();

		if (!assetReaderCollection.TryGetReader(Extension, out var reader) || reader != this) {
			assetReaderCollection.RegisterReader(this, Extension);
		}
	}

	void ILoadable.Unload()
	{
		var assetReaderCollection = Main.instance.Services.Get<AssetReaderCollection>();

		if (assetReaderCollection.TryGetReader(Extension, out var reader) && reader == this) {
			assetReaderCollection.RemoveExtension(Extension);
		}
	}

	private static unsafe IntPtr ReadFunction(IntPtr ptr, IntPtr size, IntPtr nmemb, IntPtr dataSource)
	{
		if (!memoryStreams.TryGetValue(dataSource, out var stream)) {
			return IntPtr.Zero;
		}

		int numBytes = (int)((nint)nmemb * (nint)size);
		var span = new Span<byte>((void*)ptr, numBytes);
		int numRead = stream.Read(span);

		return (IntPtr)numRead;
	}

	private static int SeekFunction(IntPtr dataSource, long offset, SeekWhence whence)
	{
		if (!memoryStreams.TryGetValue(dataSource, out var stream)) {
			return 0;
		}

		var seekOrigin = whence switch {
			SeekWhence.TF_SEEK_SET => SeekOrigin.Begin,
			SeekWhence.TF_SEEK_CUR => SeekOrigin.Current,
			SeekWhence.TF_SEEK_END => SeekOrigin.End,
			_ => throw new InvalidDataException($"{nameof(SeekWhence)} value made no sense"),
		};
		long newPosition = stream.Seek(offset, seekOrigin);

		return (int)newPosition;
	}

	private static int CloseFunction(IntPtr dataSource)
	{
		if (!memoryStreams.Remove(dataSource, out var stream)) {
			return 0;
		}

		stream.Dispose();
		Marshal.FreeHGlobal(dataSource);

		return 1;
	}
}
