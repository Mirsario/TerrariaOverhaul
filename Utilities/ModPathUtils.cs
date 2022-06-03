using System;

namespace TerrariaOverhaul.Utilities
{
	public static class ModPathUtils
	{
		public static string GetPath(Type type) => $"{GetDirectory(type)}.{type.Name}";
		
		public static string GetDirectory(Type type)
		{
			string spaceName = type.Namespace ?? string.Empty;
			int firstPeriod = spaceName.IndexOf('.');

			if (firstPeriod < 0) {
				return spaceName;
			}

			return spaceName.Substring(firstPeriod + 1).Replace('.', '/');
		}
	}
}
