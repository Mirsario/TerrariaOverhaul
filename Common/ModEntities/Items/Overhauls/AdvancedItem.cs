using Terraria;
using TerrariaOverhaul.Common.ModEntities.Players.Rendering;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public abstract partial class AdvancedItem : ItemOverhaul
	{
		public virtual float OnUseVisualRecoil => 0f;
		public virtual ScreenShake OnUseScreenShake => default;

		/*public override void Load()
		{
			On.Terraria.Player.ItemCheck_StartActualUse += (orig, player, item) => {
				WrapCall(item, () => orig(player, item));
			};
		}*/
		public override bool UseItem(Item item, Player player)
		{
			var screenShake = OnUseScreenShake;

			if(screenShake.power > 0f && screenShake.time > 0f) {
				screenShake.position = player.Center;

				ScreenShakeSystem.New(screenShake);
			}

			float visualRecoil = OnUseVisualRecoil;

			if(visualRecoil != 0f) {
				player.GetModPlayer<PlayerHoldOutAnimation>().visualRecoil += visualRecoil;
			}

			return true;
		}

		/*private static void WrapCall(Item item, Action action)
		{
			var useSound = item.UseSound;

			SwapUseSound(item);

			action();

			item.UseSound = useSound;
		}*/
	}
}
