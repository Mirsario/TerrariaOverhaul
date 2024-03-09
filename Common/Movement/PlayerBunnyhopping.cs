using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerBunnyhopping : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableBunnyhopping = new(ConfigSide.Both, "Movement", nameof(EnableBunnyhopping), () => true);

	public static float DefaultBoost => 0.685f;

	public uint NumTicksOnGround { get; set; }
	public float Boost { get; set; }

	public override void ResetEffects()
	{
		Boost = DefaultBoost;
	}

	public override bool PreItemCheck()
	{
		if (!EnableBunnyhopping) {
			return base.PreItemCheck();
		}

		bool onGround = Player.OnGround();
		bool wasOnGround = Player.WasOnGround();

		if (!onGround && wasOnGround && NumTicksOnGround < 3) {
			float boostAdd = 0f;
			float boostMultiplier = 1f;

			IPlayerOnBunnyhopHook.Invoke(Player, ref boostAdd, ref boostMultiplier);

			float totalBoost = (Boost + boostAdd) * boostMultiplier;
			float keyDirection = Player.KeyDirection().X;

			if (boostAdd != 0f || boostMultiplier != 1.0f) {
				if (keyDirection == 0f) {
					keyDirection = Player.direction;
				}
			}


			Player.velocity.X += keyDirection * totalBoost;
		}

		if (onGround) {
			NumTicksOnGround++;
		} else {
			NumTicksOnGround = 0;
		}

		return base.PreItemCheck();
	}
}
