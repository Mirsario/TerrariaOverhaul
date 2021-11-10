using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ItemUseScreenShake : ItemComponent
	{
		public ScreenShake ScreenShake { get; set; } = new(2f, 0.5f);

		public override bool? UseItem(Item item, Player player)
		{
			if (Enabled) {
				var screenShake = ScreenShake;

				if (screenShake.power > 0f && screenShake.time > 0f) {
					screenShake.position = player.Center;

					ScreenShakeSystem.New(screenShake);
				}
			}

			return base.UseItem(item, player);
		}
	}
}
