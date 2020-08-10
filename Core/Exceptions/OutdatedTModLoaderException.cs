using System;

namespace TerrariaOverhaul.Core.Exceptions
{
	public class OutdatedTModLoaderException : Exception
	{
		public OutdatedTModLoaderException(Version minVersion) : base(GetMessage(minVersion)) { }

		public static string GetMessage(Version minVersion) => $"Please update your tModLoader to at least '{minVersion}'.";
	}
}
