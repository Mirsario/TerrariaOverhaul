// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
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

                if (!Main.dedServ) {
                        item.EnableComponent<ItemAimRecoil>();
                        item.EnableComponent<ItemMuzzleflashes>(c => {
                                c.DefaultMuzzleflashLength = (uint)(item.useTime + 1);
                        });
                }
        }

        public override bool? UseItem(Item item, Player player)
        {
                if (Guns.EnableGunSoundReplacements && (!soundId.IsValid || !SoundEngine.TryGetActiveSound(soundId, out _))) {
                        soundId = SoundEngine.PlaySound(FireSound, player.Center);
                        player.GetModPlayer<FlamethrowerSoundTracker>().TrackSound(soundId);
                }

                return base.UseItem(item, player);
        }

        public override void UpdateInventory(Item item, Player player)
        {
                base.UpdateInventory(item, player);

                UpdateSound(item, player);
        }

        public override void HoldItem(Item item, Player player)
        {
                base.HoldItem(item, player);

                UpdateSound(item, player);
        }

        private void UpdateSound(Item item, Player player)
        {
                if (!Guns.EnableGunSoundReplacements || !soundId.IsValid || !SoundEngine.TryGetActiveSound(soundId, out var activeSound)) {
                        return;
                }

                bool shouldStop =
                        player.HeldItem != item ||              // Not holding this item
                        !player.ItemAnimationActive ||          // Not actively using
                        player.itemTime <= 0 ||                 // Use time expired
                        player.dead ||                          // Dead
                        player.frozen ||                        // Frozen
                        player.stoned ||                        // Petrified
                        player.webbed ||                        // Webbed
                        player.noItems;                         // Can't use items

                if (shouldStop) {
                        activeSound.Stop();
                        soundId = SlotId.Invalid;
                        player.GetModPlayer<FlamethrowerSoundTracker>().UntrackSound();
                } else {
                        activeSound.Position = player.Center;
                }
        }
}

[Autoload(Side = ModSide.Client)]
internal sealed class FlamethrowerSoundTracker : ModPlayer
{
        private SlotId trackedSound;

        public void TrackSound(SlotId soundId) => trackedSound = soundId;
        public void UntrackSound() => trackedSound = SlotId.Invalid;

        public override void PostUpdate()
        {
                // Safety net: stop orphaned sounds from dropped items or edge cases
                // that UpdateInventory/HoldItem missed
                if (!trackedSound.IsValid || !SoundEngine.TryGetActiveSound(trackedSound, out var activeSound)) {
                        return;
                }

                bool playerIsValid =
                        !Player.dead &&
                        !Player.frozen &&
                        !Player.stoned &&
                        !Player.webbed &&
                        !Player.noItems &&
                        Player.ItemAnimationActive &&
                        Player.HeldItem?.useAmmo == AmmoID.Gel;

                if (!playerIsValid) {
                        activeSound.Stop();
                        trackedSound = SlotId.Invalid;
                }
        }
}
