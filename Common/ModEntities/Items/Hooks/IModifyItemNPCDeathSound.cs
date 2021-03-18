using Terraria;
using Terraria.Audio;

namespace TerrariaOverhaul.Common.ModEntities.Items.Hooks
{
	public interface IModifyItemNPCDeathSound
	{
		void ModifyItemNPCDeathSound(Item item, Player player, NPC target, ref SoundStyle customDeathSound, ref bool playNPCDeathSound);
	}
}
