// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Guns;

public class StarCannon : ItemOverhaul
{
	public static readonly ConfigEntry<bool> EnableStarCannonRecoilHovering = new(ConfigSide.Both, true, "Guns");
	public static readonly ConfigEntry<bool> EnableStarCannonDynamicFireRate = new(ConfigSide.Both, true, "Guns");

	public static readonly SoundStyle FireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/StarCannon/StarCannonFire") {
		Volume = 0.2f,
		PitchVariance = 0.2f,
	};

	private float speedFactor;

	public virtual float MinSpeedFactor => 0.333f;
	public virtual float AccelerationTime => 1f;
	public virtual float DecelerationTime => 1f;

	public override bool ShouldApplyItemOverhaul(Item item)
	{
		if (item.useAmmo != AmmoID.FallenStar) {
			return false;
		}

		if (item.UseSound != null && item.UseSound != SoundID.Item9) {
			return false;
		}

		return true;
	}

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		speedFactor = MinSpeedFactor;

		if (Guns.EnableGunSoundReplacements) {
			item.UseSound = FireSound;
		}

		if (EnableStarCannonRecoilHovering) {
			item.EnableComponent<ItemUseVelocityRecoil>(e => {
				e.BaseVelocity = new(4.0f, 20.85f);
				e.MaxVelocity = new(3.0f, 5.0f);
			});
		}

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
			item.EnableComponent<ItemPlaySoundOnEveryUse>();
		}
	}

	public override float UseSpeedMultiplier(Item item, Player player)
	{
		float result = base.UseSpeedMultiplier(item, player);

		if (EnableStarCannonDynamicFireRate) {
			result *= speedFactor;
		}

		return result;
	}

	public override void HoldItem(Item item, Player player)
	{
		base.HoldItem(item, player);

		if (player.controlUseItem) {
			speedFactor = MathUtils.StepTowards(speedFactor, 1f, AccelerationTime * TimeSystem.LogicDeltaTime);
		} else {
			speedFactor = MinSpeedFactor;
		}
	}
}
