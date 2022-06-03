using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Guns
{
	public class Flamethrower : ItemOverhaul
	{
		private static readonly SoundStyle FireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Flamethrower/FlamethrowerFireLoop") {
			IsLooped = true,
			Volume = 0.15f,
			PitchVariance = 0.2f,
		};

		private SlotId soundId;

		public override bool ShouldApplyItemOverhaul(Item item)
			=> item.useAmmo == AmmoID.Gel;

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			item.UseSound = null;

			if (!Main.dedServ) {
				item.EnableComponent<ItemAimRecoil>();
				item.EnableComponent<ItemCrosshairController>();

				item.EnableComponent<ItemMuzzleflashes>(c => {
					c.DefaultMuzzleflashLength = (uint)(item.useTime + 1);
				});

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
			if (!soundId.IsValid || !SoundEngine.TryGetActiveSound(soundId, out _)) {
				soundId = SoundEngine.PlaySound(FireSound, player.Center);
			}

			return base.UseItem(item, player);
		}

		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			if (SoundEngine.TryGetActiveSound(soundId, out var activeSound)) {
				if (!player.ItemAnimationActive && player.itemTime <= 0) {
					activeSound.Stop();

					soundId = SlotId.Invalid;
				} else {
					activeSound.Position = player.Center;
				}
			}
		}
	}
}
