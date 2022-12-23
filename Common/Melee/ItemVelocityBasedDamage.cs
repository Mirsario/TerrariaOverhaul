using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.CombatTexts;
using TerrariaOverhaul.Content.Buffs;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemVelocityBasedDamage : ItemComponent
{
	private static readonly Gradient<Color> damageScaleColor = new(
		(0.000f, Color.White),
		(0.125f, Color.LightGreen),
		(0.250f, Color.Green),
		(0.375f, Color.YellowGreen),
		(0.500f, Color.Yellow),
		(0.625f, Color.Gold),
		(0.750f, Color.Orange),
		(0.875f, Color.OrangeRed),
		(1.000f, Color.Red)
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

	public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
	{
		if (!Enabled) {
			return;
		}

		float velocityFactor = CalculateVelocityFactor(player.velocity);
		float velocityDamageScale = CalculateDamageMultiplier(velocityFactor);

		knockback *= velocityDamageScale;
		damage = (int)Math.Round(damage * velocityDamageScale);

		if (Main.dedServ) {
			return;
		}

		bool critCopy = crit;

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
