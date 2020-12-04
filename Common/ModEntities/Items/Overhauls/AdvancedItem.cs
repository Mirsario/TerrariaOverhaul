using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.ModEntities.Players.Rendering;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public abstract class AdvancedItem : ItemOverhaul
	{
		private bool playSoundOnEveryUse;

		public virtual float OnUseVisualRecoil => 0f;
		public virtual ScreenShake OnUseScreenShake => default;
		public bool PlaySoundOnEveryUse {
			get => playSoundOnEveryUse;
			set => ItemID.Sets.SkipsInitialUseSound[item.type] = playSoundOnEveryUse = value;
		}

		//
		public override bool? UseItem(Item item, Player player)
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

			return base.UseItem(item, player);
		}
	}
}
