using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Buffs
{
	public class ManaChannelling : ModBuff
	{
		public override void SetDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			CanBeCleared = false;
		}
	}
}
