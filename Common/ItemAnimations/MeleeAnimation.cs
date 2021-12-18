using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ItemAnimations
{
	public abstract class MeleeAnimation : ILoadable
	{
		public abstract float GetItemRotation(Player player, Item item);

		void ILoadable.Load(Mod mod) { }
		void ILoadable.Unload() { }
	}
}
