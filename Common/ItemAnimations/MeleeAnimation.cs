using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ItemAnimations
{
	public abstract class MeleeAnimation : ILoadable
	{
		public abstract float GetItemRotation(Item item, Player player);

		void ILoadable.Load(Mod mod) { }
		void ILoadable.Unload() { }
	}
}
