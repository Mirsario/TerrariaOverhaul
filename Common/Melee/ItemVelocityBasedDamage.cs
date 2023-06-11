using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.CombatTexts;
using TerrariaOverhaul.Content.Buffs;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;
using static TerrariaOverhaul.Utilities.ColorUtils;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemVelocityBasedDamage : ItemComponent
{
	private static readonly Gradient<Color> damageScaleColor = new(
		(0.000f, FromHexRgba(0x_c8cdda_ff)),
		(0.100f, FromHexRgba(0x_dbe5fd_ff)),
		(0.200f, FromHexRgba(0x_ffffff_ff)),
		(0.300f, FromHexRgba(0x_ffee6e_ff)),
		(0.400f, FromHexRgba(0x_ffc100_ff)),
		(0.500f, FromHexRgba(0x_ff9a00_ff)),
		(0.600f, FromHexRgba(0x_ff781b_ff)),
		(0.700f, FromHexRgba(0x_ff551f_ff)),
		(0.800f, FromHexRgba(0x_ff3333_ff)),
		(0.900f, FromHexRgba(0x_e51842_ff)),
		(1.000f, FromHexRgba(0x_c11153_ff))
	);

	private static int visualBuffType;

	public float MinMultiplier { get; set; } = 1.0f;
	public float MaxMultiplier { get; set; } = 2.0f;
	public float MinMultiplierSpeed { get; set; } = 0.9f;
	public float MaxMultiplierSpeed { get; set; } = 12.00f;

	public override void SetStaticDefaults()
	{
		visualBuffType = ModContent.BuffType<HackAndSlash>();
	}

	public override void HoldItem(Item item, Player player)
	{
		if (Enabled && player.IsLocal() && visualBuffType > 0) {
			player.AddBuff(visualBuffType, 5, quiet: true);
		}
	}

	public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (!Enabled) {
			return;
		}

		float velocityFactor = CalculateVelocityFactor(player.velocity);
		float velocityDamageScale = CalculateDamageMultiplier(velocityFactor);

		modifiers.Knockback *= velocityDamageScale;
		modifiers.FinalDamage *= velocityDamageScale;

		if (Main.dedServ) {
			return;
		}

		CombatTextSystem.AddFilter(1, text => {
			if (uint.TryParse(text.text, out _)) {
				text.color = GetColorForVelocityFactor(velocityFactor);
			}
		});
	}

	public float CalculateVelocityFactor(Vector2 velocity)
	{
		float speed = velocity.Length();

		if (float.IsNaN(speed)) {
			return 0f;
		}

		float minSpeed = MinMultiplierSpeed;
		float maxSpeed = MaxMultiplierSpeed;
		float maxMinusMin = maxSpeed - minSpeed;

		if (maxMinusMin <= 0f) {
			return 0f;
		}

		float factor = MathF.Min(MathF.Max(speed - minSpeed, 0f) / maxMinusMin, 1f);

		return factor;
	}

	public float CalculateDamageMultiplier(Vector2 velocity)
		=> CalculateDamageMultiplier(CalculateVelocityFactor(velocity));

	public float CalculateDamageMultiplier(float velocityFactor)
	{
		float multiplier = MathHelper.Lerp(MinMultiplier, MaxMultiplier, velocityFactor);

		return multiplier;
	}

	public static Color GetColorForVelocityFactor(float velocityFactor)
	{
		return damageScaleColor.GetValue(velocityFactor);
	}
}
