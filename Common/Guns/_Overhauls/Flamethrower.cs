// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Guns;

internal class Flamethrower : ItemOverhaul
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

		//if (Guns.EnableGunSoundReplacements) {
		//	item.UseSound = null;
		//}

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
			item.EnableComponent<ItemMuzzleflashes>(c => {
				c.DefaultMuzzleflashLength = (uint)(item.useTime + 1);
			});
		}
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (Guns.EnableGunSoundReplacements && !soundId.IsValid || !SoundEngine.TryGetActiveSound(soundId, out _)) {
			soundId = SoundEngine.PlaySound(FireSound, player.Center);
		}

		return base.UseItem(item, player);
	}

	public override void UpdateInventory(Item item, Player player)
	{
		base.UpdateInventory(item, player);

		if (Guns.EnableGunSoundReplacements && SoundEngine.TryGetActiveSound(soundId, out var activeSound)) {
			if (player.HeldItem != item) {
				activeSound.Stop();

				soundId = SlotId.Invalid;
			}
		}
	}

	public override void HoldItem(Item item, Player player)
	{
		base.HoldItem(item, player);

		if (Guns.EnableGunSoundReplacements && SoundEngine.TryGetActiveSound(soundId, out var activeSound)) {
			if (!player.ItemAnimationActive && player.itemTime <= 0) {
				activeSound.Stop();

				soundId = SlotId.Invalid;
			} else {
				activeSound.Position = player.Center;
			}
		}
	}
}
