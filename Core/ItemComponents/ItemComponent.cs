using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.ItemComponents
{
	/// <summary>
	/// A <see cref="GlobalItem"/> that can be enabled and disabled at will.
	/// <br/> Might be removed in the future in favor of TML redesigns/additions.
	/// </summary>
	public abstract class ItemComponent : GlobalItem
	{
		// Unfortunately, this currently has to be checked in every override. Sucks.
		public bool Enabled { get; set; }

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return base.Clone(item, itemClone);
		}
	}
}
