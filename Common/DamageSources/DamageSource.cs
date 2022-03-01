using Terraria;

namespace TerrariaOverhaul.Common.DamageSources
{
	public sealed class DamageSource
	{
		public readonly Entity Source;
		public readonly DamageSource Parent;

		public DamageSource(Entity source, DamageSource parent = null)
		{
			Source = source;
			Parent = parent;
		}
	}
}
