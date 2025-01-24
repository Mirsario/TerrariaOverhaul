// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Archery;

public partial class Bow : ItemOverhaul
{
	public static readonly ConfigEntry<bool> EnableBowFiringDelays = new(ConfigSide.Both, true, "Archery");
	public static readonly ConfigEntry<bool> EnableBowSoundReplacements = new(ConfigSide.ClientOnly, true, "Archery");
	public static readonly ConfigEntry<bool> EnableBowChargedShots = new(ConfigSide.Both, true, "Archery");
	public static readonly ConfigEntry<bool> EnableBowChargeHovering = new (ConfigSide.Both, true, "Archery");
	public static readonly ConfigEntry<bool> EnableBowHoverJumps = new (ConfigSide.Both, true, "Archery");

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		return ArcheryWeapons.IsArcheryWeapon(item, out var kind) && kind == ArcheryWeapons.Kind.Bow;
	}

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		if (EnableBowSoundReplacements && item.UseSound == SoundID.Item5) {
			item.UseSound = ArcheryWeapons.FireSound;
		}

		if (EnableBowChargeHovering) {
			item.EnableComponent<ItemPowerAttackHover>(c => {
				c.ActivationVelocityRange = new Vector4(
					// Minimum X & Y
					float.NegativeInfinity, float.NegativeInfinity,
					// Maximum X & Y
					float.PositiveInfinity, 3.0f
				);

				if (EnableBowHoverJumps) {
					c.ControlsVelocityRecoil = true;
				}
			});
		}

		item.EnableComponent<ItemUseVelocityRecoil>(e => {
			e.BaseVelocity = new(15.0f, 15.0f);
			e.MaxVelocity = new(6.0f, 8.5f);
		}).SetEnabled(item, false);

		if (EnableBowChargedShots) {
			item.EnableComponent<ItemPowerAttacks>(c => {
				c.CanRelease = true;
				c.ChargeLengthMultiplier = 2.0f;

				var weakest = new CommonStatModifiers {
					ProjectileSpeedMultiplier = 0.25f,
				};
				var strongest = new CommonStatModifiers {
					ProjectileDamageMultiplier = 2.0f,
					ProjectileKnockbackMultiplier = 2.0f,
					ProjectileSpeedMultiplier = 3.0f,
				};

				c.StatModifiers.Gradient = new(stackalloc Gradient<CommonStatModifiers>.Key[] {
					new(0.000f, weakest),
					//new(0.750f, strongest),
					new(1.000f, strongest),
				});
			});
		}

		if (EnableBowFiringDelays) {
			item.EnableComponent<ItemPrimaryUseCharging>(c => {
				// One third of the vanilla use time is spent on the charge, two thirds remain.
				c.UseLengthMultiplier = 2f / 3f;
				c.ChargeLengthMultiplier = 1f / 3f;
			});
		}

		if (!Main.dedServ) {
			item.EnableComponent<ItemArrowRendering>(c => {
				
			});

			item.EnableComponent<ItemPowerAttackScreenShake>(c => {
				static float ScreenShakePowerFunction(float progress)
				{
					const float StartOffset = 0.00f;
					const float MaxPower = 0.15f;
					const float PowX = 7f;

					return MathHelper.Clamp((MathF.Pow(progress, PowX) * (1f + StartOffset)) - StartOffset, 0f, 1f) * MaxPower;
				}

				c.ScreenShake = new ScreenShake(ScreenShakePowerFunction, float.PositiveInfinity);
			});

			item.EnableComponent<ItemPowerAttackSounds>(c => {
				c.Sound = ArcheryWeapons.ChargeSound;
				c.CancelPlaybackOnEnd = true;
			});
		}
	}

	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		base.ModifyTooltips(item, tooltips);

		IEnumerable<string> GetCombatInfo()
		{
			yield return Mod.GetTextValue("ItemOverhauls.Archery.ManualCharging");
			yield return Mod.GetTextValue("ItemOverhauls.Archery.HoverShots");
		}

		TooltipUtils.ShowCombatInformation(Mod, tooltips, GetCombatInfo);
	}
}
