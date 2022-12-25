using System.Collections.Generic;
using System.Reflection;

namespace TerrariaOverhaul.Utilities;

internal static class AssemblyExtensions
{
	private static readonly Dictionary<int, bool> isDebugCheckCache = new();

	public static bool IsDebugAssembly(this Assembly assembly)
	{
		int hash = assembly.GetHashCode();

		if (isDebugCheckCache.TryGetValue(hash, out bool result)) {
			return result;
		}

		var attribute = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();

		isDebugCheckCache[hash] = result = attribute?.Configuration?.Contains("Debug") == true;

		return result;
	}
}
