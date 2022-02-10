using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Buffs
{
	public class DodgeTiredness : ModBuff
	{
		public override void SetStaticDefaults()
		{
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		}
	}
}
