using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerBunnyhopCombos : ModPlayer, IPlayerOnBunnyhopHook
{
	public static readonly SoundStyle BunnyhopComboSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/BunnyhopCombo") {
		Volume = 0.175f,
	};
	public static readonly SoundStyle BunnyhopComboBreakSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/BunnyhopComboBreak") {
		Volume = 0.3f,
		PitchVariance = 0.1f,
	};

	private const string AudioEnabledSaveKey = "BunnyhopComboAudioEnabled";

	private float lastHorizontalSpeedAbs;
	private uint numTicksPlayerWereSlowFor;

	public float BoostBonusPerCombo { get; set; } // Must be set by an accessory
	public float MaxBoostBonus { get; set; }
	public bool AudioEnabled { get; set; } = true;
	public uint Combo { get; private set; }

	public override void ResetEffects()
	{
		BoostBonusPerCombo = 0f;
		MaxBoostBonus = 5f;
	}

	public override void PostItemCheck()
	{
		if (!UpdateCombo()) {
			EndCombo();
		}
	}

	public override void SaveData(TagCompound tag)
	{
		tag[AudioEnabledSaveKey] = AudioEnabled;
	}

	public override void LoadData(TagCompound tag)
	{
		if (tag.ContainsKey(AudioEnabledSaveKey)) {
			AudioEnabled = tag.GetBool(AudioEnabledSaveKey);
		}
	}

	void IPlayerOnBunnyhopHook.OnBunnyhop(Player player, ref float boostAdd, ref float boostMultiplier)
	{
		if (BoostBonusPerCombo <= 0f) {
			return;
		}

		Player.TryGetModPlayer(out PlayerDodgerolls dodgerolls);
		Player.TryGetModPlayer(out PlayerClimbing climbing);
		
		if (Math.Abs(player.velocity.X) <= 3f
		&& !player.controlRight
		&& !player.controlLeft
		&& dodgerolls?.IsDodging != true
		&& climbing?.IsClimbing != true) {
			EndCombo();
			return;
		}

		boostAdd += Combo * BoostBonusPerCombo;

		Combo++;

		/*
		if (dodgerolls?.IsDodging == true) {
			Combo++;
		}
		*/

		if (!Main.dedServ && Player.IsLocal()) {
			float lerpStep = MathF.Min(1f, Combo / 30f);

			/*if (Combo >= 1) {
				var color = Color.Lerp(Color.White, Color.HotPink, lerpStep);

				CombatText.NewText(Player.Bottom.ToRectangle(1, 1), color, $"x{Combo}!");
			}
			*/

			if (AudioEnabled) {
				var soundStyle = BunnyhopComboSound
					.WithPitchOffset(MathHelper.Lerp(-1.0f, 0.5f, lerpStep))
					.WithVolumeScale(Player.IsLocal() ? 1f : 0.5f);

				SoundEngine.PlaySound(soundStyle, Player.Center);
			}
		}
	}

	public void EndCombo()
	{
		if (Combo <= 0) {
			return;
		}

		if (AudioEnabled) {
			if (!Main.dedServ && Player.IsLocal()) {
				SoundEngine.PlaySound(BunnyhopComboBreakSound, Player.Center);
			}
		}

		Combo = 0;
	}

	private bool UpdateCombo()
	{
		float horizontalSpeed = Math.Abs(Player.velocity.X);
		float horizontalSpeedDelta = lastHorizontalSpeedAbs - horizontalSpeed;

		lastHorizontalSpeedAbs = horizontalSpeed;

		bool isSlow = horizontalSpeed < 3f;

		if (isSlow) {
			numTicksPlayerWereSlowFor++;
		} else {
			numTicksPlayerWereSlowFor = 0;
		}

		// Cancelation checks

		if (Combo <= 0) {
			return true;
		}

		if (Player.TryGetModPlayer(out PlayerBunnyhopping bunnyhopping)) {
			if (bunnyhopping.NumTicksOnGround >= 3) {
				return false;
			}
		}

		if (isSlow) {
			// End combo immediately if slow while on the ground
			if (Player.OnGround()) {
				return false;
			}

			// End combo if have been slow for a bit while in the air.
			if (numTicksPlayerWereSlowFor >= 60) {
				return false;
			}
			
			// End combo if the player's speed suddenly disappeared.
			if (horizontalSpeedDelta < -1f && horizontalSpeed < 1f) {
				return false;
			}
		}

		return true;
	}
}
