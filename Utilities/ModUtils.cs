using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaOverhaul.Utilities
{
	public static class ModUtils
	{
		public static string GetTypePath(Type type) => type.FullName.Replace('.', '/');
	}
}
