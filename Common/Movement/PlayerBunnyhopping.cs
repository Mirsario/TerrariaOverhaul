using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerBunnyhopping : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableBunnyhopping = new(ConfigSide.Both, "PlayerMovement", nameof(EnableBunnyhopping), () => true);

	public static float DefaultBoost => 0.8f;

	public float Boost { get; set; }

	public override void ResetEffects()
	{
		Boost = DefaultBoost;
	}

	public override void PostItemCheck()
	{
		if (!EnableBunnyhopping) {
			return;
		}

		bool onGround = Player.OnGround();
		bool wasOnGround = Player.WasOnGround();

		if (onGround || !wasOnGround || Player.velocity.Y >= 0f) {
			return;
		}

		float boost = Boost;
		
		IPlayerOnBunnyhopHook.Invoke(Player, ref boost);

		Player.velocity.X += boost * Player.KeyDirection().X;
	}
}
