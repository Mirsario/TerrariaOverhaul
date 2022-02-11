using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Guns
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ItemMuzzleflashes : ItemComponent
	{
		public Timer MuzzleflashTimer;
		public Color MuzzleflashColor = Color.White;
		public Vector3 LightColor = Color.Gold.ToVector3();
		public uint DefaultMuzzleflashLength = 5;

		public override bool? UseItem(Item item, Player player)
		{
			if (Enabled) {
				MuzzleflashTimer.Set(DefaultMuzzleflashLength);
			}

			return base.UseItem(item, player);
		}
	}
}
