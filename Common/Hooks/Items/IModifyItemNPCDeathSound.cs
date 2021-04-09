using Terraria;
using Terraria.Audio;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	public interface IModifyItemNPCDeathSound
	{
		void ModifyItemNPCDeathSound(Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound);
	}
}
