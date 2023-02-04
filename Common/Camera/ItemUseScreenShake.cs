using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Camera;

[Autoload(Side = ModSide.Client)]
public sealed class ItemUseScreenShake : ItemComponent
{
	public ScreenShake ScreenShake { get; set; } = new(0.2f, 0.25f);

	public override bool? UseItem(Item item, Player player)
	{
		if (Enabled) {
			var screenShake = ScreenShake;

			ScreenShakeSystem.New(screenShake, player.Center);
		}

		return base.UseItem(item, player);
	}
}
