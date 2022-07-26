using System;

namespace TerrariaOverhaul.Utilities;

public static class ModUtils
{
	public static string? GetTypePath(Type type)
		=> type.FullName?.Replace('.', '/');
}
