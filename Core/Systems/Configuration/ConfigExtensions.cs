namespace TerrariaOverhaul.Core.Systems.Configuration
{
	public static class ConfigExtensions
	{
		public static T Local<T>(this T config) where T : Config
			=> (T)config.Local;

		public static T Server<T>(this T config) where T : Config
			=> (T)config.Server;
	}
}
