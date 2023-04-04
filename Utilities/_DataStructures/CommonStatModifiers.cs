using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

[StructLayout(LayoutKind.Sequential, Size = SizeConstant)]
public struct CommonStatModifiers
{
	private const int SizeInFloats = 6;
	private const int SizeConstant = sizeof(float) * SizeInFloats;

	public static readonly int Size = SizeConstant;
	public static readonly CommonStatModifiers Default = new();

	public float MeleeDamageMultiplier = 1f;
	public float MeleeKnockbackMultiplier = 1f;
	public float MeleeRangeMultiplier = 1f;
	public float ProjectileDamageMultiplier = 1f;
	public float ProjectileKnockbackMultiplier = 1f;
	public float ProjectileSpeedMultiplier = 1f;

	public CommonStatModifiers() { }

	static CommonStatModifiers()
	{
		Gradient<CommonStatModifiers>.Lerp = Lerp;
	}

	public static CommonStatModifiers Lerp(CommonStatModifiers a, CommonStatModifiers b, float step)
		=> Lerp(in a, in b, step);

	public static CommonStatModifiers Lerp(in CommonStatModifiers a, in CommonStatModifiers b, float step)
	{
		CommonStatModifiers result;

		result.MeleeRangeMultiplier = MathHelper.Lerp(a.MeleeRangeMultiplier, b.MeleeRangeMultiplier, step);
		result.MeleeDamageMultiplier = MathHelper.Lerp(a.MeleeDamageMultiplier, b.MeleeDamageMultiplier, step);
		result.MeleeKnockbackMultiplier = MathHelper.Lerp(a.MeleeKnockbackMultiplier, b.MeleeKnockbackMultiplier, step);
		result.ProjectileSpeedMultiplier = MathHelper.Lerp(a.ProjectileSpeedMultiplier, b.ProjectileSpeedMultiplier, step);
		result.ProjectileDamageMultiplier = MathHelper.Lerp(a.ProjectileDamageMultiplier, b.ProjectileDamageMultiplier, step);
		result.ProjectileKnockbackMultiplier = MathHelper.Lerp(a.ProjectileKnockbackMultiplier, b.ProjectileKnockbackMultiplier, step);

		return result;
	}

	public static CommonStatModifiers operator *(in CommonStatModifiers a, float factor)
	{
		CommonStatModifiers result;

		result.MeleeRangeMultiplier = a.MeleeRangeMultiplier * factor;
		result.MeleeDamageMultiplier = a.MeleeDamageMultiplier * factor;
		result.MeleeKnockbackMultiplier = a.MeleeKnockbackMultiplier * factor;
		result.ProjectileSpeedMultiplier = a.ProjectileSpeedMultiplier * factor;
		result.ProjectileDamageMultiplier = a.ProjectileDamageMultiplier * factor;
		result.ProjectileKnockbackMultiplier = a.ProjectileKnockbackMultiplier * factor;

		return result;
	}

	public static CommonStatModifiers operator *(in CommonStatModifiers a, in CommonStatModifiers b)
	{
		CommonStatModifiers result;

		result.MeleeRangeMultiplier = a.MeleeRangeMultiplier * b.MeleeRangeMultiplier;
		result.MeleeDamageMultiplier = a.MeleeDamageMultiplier * b.MeleeDamageMultiplier;
		result.MeleeKnockbackMultiplier = a.MeleeKnockbackMultiplier * b.MeleeKnockbackMultiplier;
		result.ProjectileSpeedMultiplier = a.ProjectileSpeedMultiplier * b.ProjectileSpeedMultiplier;
		result.ProjectileDamageMultiplier = a.ProjectileDamageMultiplier * b.ProjectileDamageMultiplier;
		result.ProjectileKnockbackMultiplier = a.ProjectileKnockbackMultiplier * b.ProjectileKnockbackMultiplier;

		return result;
	}
}
