using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerBunnyhopping : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableBunnyhopping = new(ConfigSide.Both, "PlayerMovement", nameof(EnableBunnyhopping), () => true);

	public static float DefaultBoost => 0.8f;

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

			Player.velocity.X += Player.KeyDirection().X * totalBoost;
		}

		if (onGround) {
			NumTicksOnGround++;
		} else {
			NumTicksOnGround = 0;
		}

		return base.PreItemCheck();
	}
}
