namespace TerrariaOverhaul.Utilities
{
	public struct CommonStatMultipliers
	{
		public static readonly CommonStatMultipliers Default = new() {
			MeleeDamageMultiplier = 1f,
			MeleeKnockbackMultiplier = 1f,
			MeleeRangeMultiplier = 1f,
			ProjectileDamageMultiplier = 1f,
			ProjectileKnockbackMultiplier = 1f,
			ProjectileSpeedMultiplier = 1f,
		};

		public float MeleeDamageMultiplier;
		public float MeleeKnockbackMultiplier;
		public float MeleeRangeMultiplier;
		public float ProjectileDamageMultiplier;
		public float ProjectileKnockbackMultiplier;
		public float ProjectileSpeedMultiplier;

		public static CommonStatMultipliers operator *(CommonStatMultipliers a, CommonStatMultipliers b)
		{
			a.MeleeDamageMultiplier *= b.MeleeDamageMultiplier;
			a.MeleeKnockbackMultiplier *= b.MeleeKnockbackMultiplier;
			a.MeleeRangeMultiplier *= b.MeleeRangeMultiplier;
			a.ProjectileDamageMultiplier *= b.ProjectileDamageMultiplier;
			a.ProjectileKnockbackMultiplier *= b.ProjectileKnockbackMultiplier;
			a.ProjectileSpeedMultiplier *= b.ProjectileSpeedMultiplier;

			return a;
		}
	}
}
