using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.Tags;

namespace TerrariaOverhaul.Common.Melee
{
	public sealed class ItemHitSoundReplacements : GlobalItem, IModifyItemNPCHitSound
	{
		public static readonly ModSoundStyle WoodenHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/WoodenHit", 3, volume: 0.3f, pitchVariance: 0.1f);

		void IModifyItemNPCHitSound.ModifyItemNPCHitSound(Item item, Player player, NPC target, ref ISoundStyle customHitSound, ref bool playNPCHitSound)
		{
			if (OverhaulItemTags.Wooden.Has(item.netID)) {
				customHitSound = WoodenHitSound;
			}
		}
	}
}
