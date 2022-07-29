using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

public sealed class ScreenShakeSystem : ModSystem
{
	public static readonly RangeConfigEntry<float> ScreenShakeStrength = new(ConfigSide.ClientOnly, "Camera", nameof(ScreenShakeStrength), 0f, 1f, () => 1f);

	private static readonly Stopwatch Stopwatch = new();
	private static readonly List<ScreenShake> ScreenShakes = new();

	public override void PostUpdateEverything()
	{
		if (Main.gamePaused && !Stopwatch.IsRunning) {
			return;
		}

		float delta = (float)Stopwatch.Elapsed.TotalSeconds;

		if (delta <= 0f) {
			delta = TimeSystem.LogicDeltaTime;
		}

		for (int i = 0; i < ScreenShakes.Count; i++) {
			var shake = ScreenShakes[i];

			shake.Time -= delta;

			if (shake.Time <= 0f) {
				ScreenShakes.RemoveAt(i--);
			} else {
				ScreenShakes[i] = shake;
			}
		}

		if (Main.gamePaused) {
			Stopwatch.Reset();
		} else {
			Stopwatch.Restart();
		}
	}

	public static float GetPowerAtPoint(Vector2 point)
	{
		float power = 0f;

		for (int i = 0; i < ScreenShakes.Count; i++) {
			var shake = ScreenShakes[i];

			float maxPower;

			if (shake.PowerGradient != null) {
				float progress = (shake.TimeMax - shake.Time) / shake.TimeMax;

				if (float.IsNaN(progress)) {
					progress = 0f;
				}

				maxPower = shake.PowerGradient.GetValue(progress);
			} else {
				maxPower = shake.Power;
			}

			if (shake.Position.HasValue) {
				maxPower *= 1f - Math.Min(1f, Vector2.Distance(shake.Position.Value, point) / shake.Range);
			}

			power += maxPower * (shake.Time / shake.TimeMax);
		}

		return power * ScreenShakeStrength.Value;
	}

	public static void New(float power, float time, Vector2? position = null, float range = ScreenShake.DefaultRange, string? uniqueId = null)
		=> New(new ScreenShake(power, time, position, range, uniqueId));

	public static void New(Gradient<float> powerGradient, float time, Vector2? position = null, float range = ScreenShake.DefaultRange, string? uniqueId = null)
		=> New(new ScreenShake(powerGradient, time, position, range, uniqueId));

	public static void New(ScreenShake screenShake)
	{
		if (screenShake.UniqueId == null) {
			ScreenShakes.Add(screenShake);
			return;
		}

		int index = ScreenShakes.FindIndex(s => s.UniqueId == screenShake.UniqueId);

		if (index >= 0) {
			ScreenShakes[index] = screenShake;
		} else {
			ScreenShakes.Add(screenShake);
		}
	}
}
