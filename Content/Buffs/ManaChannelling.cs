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

			DisplayName.SetDefault("Mana Channelling");
			Description.SetDefault("Your patience is increasing your mana regeneration");
		}
	}
}
