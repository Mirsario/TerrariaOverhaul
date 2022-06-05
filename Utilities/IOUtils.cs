using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace TerrariaOverhaul.Utilities
{
	public static class IOUtils
	{
		public static bool TryReadAllTextSafely([NotNullWhen(true)] string? filePath, [NotNullWhen(true)] out string? result, int maxAttempts = 20)
		{
			if (!File.Exists(filePath)) {
				result = null;

				return false;
			}

			for (int i = 0; i < maxAttempts; i++) {
				try {
					result = File.ReadAllText(filePath);

					return true;
				}
				catch {
					Thread.Sleep(50);
				}
			}

			result = null;

			return false;
		}
	}
}
