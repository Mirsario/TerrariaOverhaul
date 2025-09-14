// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.Melee;

internal sealed class ItemHitSoundReplacements : GlobalItem, IModifyItemNPCHitSound
{
	public static readonly SoundStyle WoodenHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/WoodenHit", 3) {
		Volume = 0.3f,
		PitchVariance = 0.1f,
	};
	private static readonly ContentSet Wooden = nameof(Wooden);

	void IModifyItemNPCHitSound.ModifyItemNPCHitSound(Item item, Player player, NPC target, ref SoundStyle? customHitSound, ref bool playNPCHitSound)
	{
		if (Wooden.Has(item)) {
			customHitSound = WoodenHitSound;
		}
	}
}
