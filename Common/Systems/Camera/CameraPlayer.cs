using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Systems.Camera
{
	public sealed class CameraPlayer : ModPlayer
	{
		public override void ModifyScreenPosition() => ModContent.GetInstance<CameraSystem>().ModifyScreenPosition(player);
	}
}
