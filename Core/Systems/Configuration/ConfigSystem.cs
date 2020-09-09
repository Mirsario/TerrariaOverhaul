using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Systems.Configuration
{
	public sealed class ConfigSystem : ModSystem
	{
		public static T GetConfig<T>() where T : Config
			=> ModContent.GetInstance<T>();
	}
}
