using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Guns
{
	public class Flamethrower : Gun
	{
		private static readonly ISoundStyle FireSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Flamethrower/FlamethrowerFireLoop", 0, volume: 0.15f, pitchVariance: 0.2f);

		private SlotId soundId;

		public override bool ShouldApplyItemOverhaul(Item item)
			=> item.useAmmo == AmmoID.Gel;

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			item.UseSound = null;

			if (!Main.dedServ) {
				item.EnableComponent<ItemUseVisualRecoil>(c => {
					c.Power = 4f;
				});

				item.EnableComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(3f, 0.2f);
				});
			}
		}

		public override bool? UseItem(Item item, Player player)
		{
			if (!soundId.IsValid || SoundEngine.GetActiveSound(soundId) == null) {
				soundId = SoundEngine.PlayTrackedSound(FireSound, player.Center);
			}

			return base.UseItem(item, player);
		}

		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			if (player.itemAnimation <= 0 && player.itemTime <= 0 && soundId.IsValid) {
				var activeSound = SoundEngine.GetActiveSound(soundId);

				activeSound?.Stop();

				soundId = SlotId.Invalid;
			} else if (soundId.IsValid) {
				var activeSound = SoundEngine.GetActiveSound(soundId);

				if (activeSound != null) {
					activeSound.Position = player.Center;
				}
			}
		}
	}
}
