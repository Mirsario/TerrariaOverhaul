using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ItemPlaySoundOnEveryUse : ItemComponent
	{
		public override bool? UseItem(Item item, Player player)
		{
			if (Enabled) {
				ItemID.Sets.SkipsInitialUseSound[item.type] = true;

				if (item.UseSound != null) {
					SoundEngine.PlaySound(item.UseSound, player.Center);
				}
			}

			return base.UseItem(item, player);
		}
	}
}
