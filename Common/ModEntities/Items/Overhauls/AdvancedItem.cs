using Terraria;
using Terraria.Audio;
using TerrariaOverhaul.Common.ModEntities.Players.Rendering;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public abstract partial class AdvancedItem : ItemOverhaul
	{
		public virtual float OnUseVisualRecoil => 0f;
		public virtual ScreenShake OnUseScreenShake => default;
		public virtual bool PlaySoundOnEveryUse => false;

		public override void Load()
		{
			On.Terraria.Player.ItemCheck_StartActualUse += (orig, player, item) => {
				if(!PlaySoundOnEveryUse) {
					orig(player, item);

					return;
				}

				var useSound = item.UseSound;

				item.UseSound = null;

				orig(player, item);

				item.UseSound = useSound;
			};
		}
		public override bool UseItem(Item item, Player player)
		{
			if(!Main.dedServ) {
				//Screenshake
				var screenShake = OnUseScreenShake;

				if(screenShake.power > 0f && screenShake.time > 0f) {
					screenShake.position = player.Center;

					ScreenShakeSystem.New(screenShake);
				}

				//Recoil
				float visualRecoil = OnUseVisualRecoil;

				if(visualRecoil != 0f) {
					player.GetModPlayer<PlayerHoldOutAnimation>().visualRecoil += visualRecoil;
				}

				//Sounds
				if(PlaySoundOnEveryUse && item.UseSound != null) {
					SoundEngine.PlaySound(item.UseSound, player.Center);
				}
			}

			return true;
		}
	}
}
