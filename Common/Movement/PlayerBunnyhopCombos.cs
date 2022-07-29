using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerBunnyhopCombos : ModPlayer, IPlayerOnBunnyhopHook
{
	public static readonly SoundStyle BunnyhopComboSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/BunnyhopCombo") {
		Volume = 0.3f,
		PitchVariance = 0.2f,
	};
	public static readonly SoundStyle BunnyhopComboBreakSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/BunnyhopComboBreak") {
		Volume = 0.3f,
		PitchVariance = 0.1f,
	};

	private float lastHorizontalSpeedAbs;

	public bool IsActive { get; set; }
	public uint Combo { get; private set; }

	public override void ResetEffects()
	{
		IsActive = false;
	}

	public override void PostItemCheck()
	{
		if (!Player.TryGetModPlayer(out PlayerBunnyhopping bunnyhopping)) {
			return;
		}
		
		if (bunnyhopping.NumTicksOnGround >= 3) {
			EndCombo();
		}

		float newHorizontalSpeedAbs = Math.Abs(Player.velocity.X);

		if (newHorizontalSpeedAbs + 1f < lastHorizontalSpeedAbs) {
			EndCombo();
		}

		lastHorizontalSpeedAbs = newHorizontalSpeedAbs;
	}

	void IPlayerOnBunnyhopHook.OnBunnyhop(Player player, ref float boostAdd, ref float boostMultiplier)
	{
		if (!IsActive) {
			return;
		}

		if (!Player.TryGetModPlayer(out PlayerBunnyhopping bunnyhopping)) {
			return;
		}

		boostAdd += Combo * 0.05f;
		
		Combo++;

		if (!Main.dedServ && Player.IsLocal()) {
			if (Combo >= 1) {
				CombatText.NewText(Player.Bottom.ToRectangle(1, 1), Color.HotPink, $"x{Combo}!");
			}

			var soundStyle = BunnyhopComboSound
				.WithPitchOffset(MathHelper.Lerp(-1.0f, 0.5f, MathF.Min(1f, Combo / 30f)))
				.WithVolumeScale(Player.IsLocal() ? 1f : 0.5f);

			SoundEngine.PlaySound(soundStyle, Player.Center);
		}
	}

	public void EndCombo()
	{
		if (Combo <= 0) {
			return;
		}

		if (!Main.dedServ && Player.IsLocal()) {
			var sound = Melee.ItemHitSoundReplacements.WoodenHitSound with {
				Pitch = 0.5f,
				PitchVariance = 0.1f,
			};

			SoundEngine.PlaySound(sound, Player.Center);
		}

		Combo = 0;
	}
}
