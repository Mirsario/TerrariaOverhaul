using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.DataStructures;

namespace TerrariaOverhaul.Common.Systems.Camera.ScreenShakes
{
	public sealed class ScreenShakeSystem : ModSystem
	{
		private static List<ScreenShake> screenShakes;

		private Stopwatch sw;

		public override void Load()
		{
			sw = new Stopwatch();
			screenShakes = new List<ScreenShake>();
		}
		public override void Unload()
		{
			screenShakes = null;
		}
		public override void PostUpdateEverything()
		{
			if(Main.gamePaused && !sw.IsRunning) {
				return;
			}

			float delta = (float)sw.Elapsed.TotalSeconds;

			if(delta <= 0f) {
				delta = TimeSystem.LogicDeltaTime;
			}

			for(int i = 0; i < screenShakes.Count; i++) {
				var shake = screenShakes[i];

				shake.time -= delta;

				if(shake.time <= 0f) {
					screenShakes.RemoveAt(i--);
				} else {
					screenShakes[i] = shake;
				}
			}

			if(Main.gamePaused) {
				sw.Reset();
			} else {
				sw.Restart();
			}
		}

		public static float GetPowerAtPoint(Vector2 point)
		{
			float power = 0f;

			for(int i = 0; i < screenShakes.Count; i++) {
				var shake = screenShakes[i];

				float maxPower;

				if(shake.powerGradient != null) {
					maxPower = shake.powerGradient.GetValue(shake.TimeMax - shake.time);
				} else {
					maxPower = shake.power;
				}

				if(shake.position.HasValue) {
					maxPower *= 1f - Math.Min(1f, Vector2.Distance(shake.position.Value, point) / shake.range);
				}

				power += maxPower * (shake.time / shake.TimeMax);
			}

			return power * CameraSystem.Config.screenShakeStrength;
		}

		public static void New(float power, float time, Vector2? position = null, float range = ScreenShake.DefaultRange, string uniqueId = null)
			=> New(new ScreenShake(power, time, position, range, uniqueId));

		public static void New(Gradient<float> powerGradient, float time, Vector2? position = null, float range = ScreenShake.DefaultRange, string uniqueId = null)
			=> New(new ScreenShake(powerGradient, time, position, range, uniqueId));

		public static void New(ScreenShake screenShake)
		{
			if(screenShake.uniqueId == null) {
				screenShakes.Add(screenShake);
				return;
			}

			int index = screenShakes.FindIndex(s => s.uniqueId == screenShake.uniqueId);

			if(index >= 0) {
				screenShakes[index] = screenShake;
			} else {
				screenShakes.Add(screenShake);
			}
		}
	}
}
