using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.Systems.Configuration;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.Systems.Camera.ScreenShakes
{
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

				shake.time -= delta;

				if (shake.time <= 0f) {
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

				if (shake.powerGradient != null) {
					float progress = (shake.TimeMax - shake.time) / shake.TimeMax;

					if (float.IsNaN(progress)) {
						progress = 0f;
					}

					maxPower = shake.powerGradient.GetValue(progress);
				} else {
					maxPower = shake.power;
				}

				if (shake.position.HasValue) {
					maxPower *= 1f - Math.Min(1f, Vector2.Distance(shake.position.Value, point) / shake.range);
				}

				power += maxPower * (shake.time / shake.TimeMax);
			}

			return power * ScreenShakeStrength.Value;
		}

		public static void New(float power, float time, Vector2? position = null, float range = ScreenShake.DefaultRange, string uniqueId = null)
			=> New(new ScreenShake(power, time, position, range, uniqueId));

		public static void New(Gradient<float> powerGradient, float time, Vector2? position = null, float range = ScreenShake.DefaultRange, string uniqueId = null)
			=> New(new ScreenShake(powerGradient, time, position, range, uniqueId));

		public static void New(ScreenShake screenShake)
		{
			if (screenShake.uniqueId == null) {
				ScreenShakes.Add(screenShake);
				return;
			}

			int index = ScreenShakes.FindIndex(s => s.uniqueId == screenShake.uniqueId);

			if (index >= 0) {
				ScreenShakes[index] = screenShake;
			} else {
				ScreenShakes.Add(screenShake);
			}
		}
	}
}
