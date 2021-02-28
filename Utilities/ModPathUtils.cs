using System;

namespace TerrariaOverhaul.Utilities
{
	public static class ModPathUtils
	{
		public static string GetPath(Type type) => $"{GetDirectory(type)}.{type.Name}";
		public static string GetDirectory(Type type)
		{
			string ns = type.Namespace;
			int firstPeriod = ns.IndexOf('.');

			if(firstPeriod < 0) {
				return ns;
			}

			return ns.Substring(firstPeriod + 1).Replace('.', '/');
		}
	}
}
