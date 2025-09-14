// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Magic;

internal partial class MagicWeapon : ItemOverhaul
{
	public static readonly ConfigEntry<bool> EnableMagicSoundReplacements = new(ConfigSide.ClientOnly, true, "Magic");
	public static readonly ConfigEntry<bool> EnableMagicPowerAttacks = new(ConfigSide.Both, true, "Magic");

	public static readonly SoundStyle MagicBlastSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicBlast", 3) {
		PitchVariance = 0.1f,
	};
	public static readonly SoundStyle MagicPowerfulBlastSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicPowerfulBlast") {
		PitchVariance = 0.4f,
	};
	public static readonly SoundStyle ChargeSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicCharge") {
		Volume = 0.5f,
		PitchVariance = 0.1f,
	};

	private static readonly Gradient<float> chargeScreenShakePowerGradient = new(
		(0.000f, 0.000f),
		(0.250f, 0.025f),
		(0.500f, 0.090f),
		(1.000f, 0.200f)
	);

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		// Ignore weapons with non-magic damage types
		if (!item.CountsAsClass(DamageClass.Magic)) {
			return false;
		}

		// Avoid tools and placeables
		if (item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
			return false;
		}

		// Ignore weapons that don't shoot, don't use mana, or deal hitbox damage 
		if (item.shoot <= ProjectileID.None || item.mana <= 0 || !item.noMelee) {
			return false;
		}

		// Ignore laser guns
		if (item.UseSound == SoundID.Item157) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		if (EnableMagicSoundReplacements && item.UseSound == SoundID.Item43) {
			item.UseSound = MagicBlastSound;
		}

		ContentSampleUtils.TryGetProjectile(item.shoot, out var shotProjectile);

		if (EnableMagicPowerAttacks) {
			item.EnableComponent<ItemPowerAttacks>(c => {
				c.ChargeLengthMultiplier = 2f;

				var modifiers = new CommonStatModifiers();

				modifiers.ProjectileDamageMultiplier = modifiers.MeleeDamageMultiplier = 1.75f;
				modifiers.ProjectileKnockbackMultiplier = modifiers.MeleeKnockbackMultiplier = 1.5f;
				modifiers.ProjectileSpeedMultiplier = 2f;

				// Workaround for Vilethorn-type projectiles stretching too far.
				// Preferably they'd be still sped up in some way, but it's all too hardcoded in vanilla.
				if (shotProjectile?.aiStyle == ProjAIStyleID.Vilethorn) {
					modifiers.ProjectileSpeedMultiplier = 1.0f;
				}

				c.StatModifiers.Single = modifiers;
			});
		}

		if (!Main.dedServ) {
			static float ScreenShakePowerFunction(float progress)
			{
				const float StartOffset = 0.05f;
				const float MaxPower = 0.3f;
				const float PowX = 3.0f;

				return MathHelper.Clamp((MathF.Pow(progress, PowX) * (1f + StartOffset)) - StartOffset, 0f, 1f) * MaxPower;
			}

			item.EnableComponent<ItemPowerAttackScreenShake>(c => {
				c.ScreenShake = new ScreenShake(ScreenShakePowerFunction, float.PositiveInfinity);
			});

			item.EnableComponent<ItemPowerAttackSounds>(c => {
				c.Sound = ChargeSound;
				c.CancelPlaybackOnEnd = true;
			});
		}
	}
}
