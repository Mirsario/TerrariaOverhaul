using Terraria;
using Terraria.Audio;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemNPCHitSound
	{
		void ModifyItemNPCHitSound(Item item, Player player, NPC target, ref SoundStyle customHitSound, ref bool playNPCHitSound);
	}
}
