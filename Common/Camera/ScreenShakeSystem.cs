using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Input;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

[Autoload(Side = ModSide.Client)]
public sealed class ScreenShakeSystem : ModSystem
{
	public static readonly RangeConfigEntry<float> ScreenShakeStrength = new(ConfigSide.ClientOnly, "Camera", nameof(ScreenShakeStrength), 0f, 1f, () => 1f);

	private static readonly List<ScreenShake> screenShakes = new();

	private static FastNoiseLite? noise;
	//private static Vector2 lastAppliedScreenShakeOffset;

	public override void Load()
	{
		noise = new FastNoiseLite();

		CameraSystem.RegisterCameraModifier(0, innerAction => {
			// Because this modifier runs very early (to not get smoothed out), it has to undo its previous frame's offset
			// For the camera smoothing system to also not pick it up.

			innerAction();

			if (Main.gameMenu) {
				return;
			}

			const float BaseScreenShakePower = 40f;

			var samplingPosition = Main.LocalPlayer?.Center ?? CameraSystem.ScreenCenter;
			float screenShakePower = GetPowerAtPoint(samplingPosition);
			var noiseOffset = GetNoiseValue();

			screenShakePower = Math.Min(screenShakePower, 1f);

			var screenShakeOffset = noiseOffset * BaseScreenShakePower * screenShakePower;

			Main.screenPosition += screenShakeOffset;

			//lastAppliedScreenShakeOffset = screenShakeOffset;

#if DEBUG
			if (InputSystem.GetKey(Microsoft.Xna.Framework.Input.Keys.Tab)) {
				New(2f, 0.5f);
			}
#endif

			// Render debug
			if (DebugSystem.EnableDebugRendering) {
				var playerCenter = Main.LocalPlayer!.Center.ToPoint();
				var topLeft = new Point(playerCenter.X - 64, playerCenter.Y - 128);
				int maxHeight = 256;
				int height = (int)(maxHeight * screenShakePower);

				DebugSystem.DrawRectangle(new Rectangle(topLeft.X, topLeft.Y, 32, maxHeight), Color.White);
				DebugSystem.DrawRectangle(new Rectangle(topLeft.X, topLeft.Y + maxHeight - height, 32, height), Color.Orange);
			}
		});
	}

	public static Vector2 GetNoiseValue()
	{
		if (noise == null) {
			return Vector2.Zero;
		}

		static float GetValueWithSeed(int seed, float x)
		{
			noise!.SetSeed(seed);

			return noise!.GetNoise(x, 0f);
		}

		float time = TimeSystem.RenderTime;


		// Basic 2D
		const float FrequencyScale = 14.0f;

		noise.SetFrequency(FrequencyScale);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

		var result = new Vector2(
			GetValueWithSeed(420, time),
			GetValueWithSeed(1337, time)
		);

		// Rotation-based
		/*
		const float RotationFrequency = 2.0f;
		const float AlternationFrequency = 5.0f;

		noise.SetFrequency(1.0f);

		float rotation = GetValueWithSeed(0, time * RotationFrequency) * MathHelper.Pi;
		float length = GetValueWithSeed(1, time * AlternationFrequency);
		//float length = MathF.Sin(time * AlternationFrequency);
		var result = new Vector2(length, 0f).RotatedBy(rotation);
		*/

		float length = result.Length();

		result = length <= 1f ? result : result / length;

		/*
		float length = result.SafeLength();

		if (length > 1f) {
			result /= length;
		}
		*/

		/*
		var spriteViewMatrix = Main.GameViewMatrix;
		var field = typeof(SpriteViewMatrix).GetField("_effectMatrix", BindingFlags.Instance | BindingFlags.NonPublic);
		var matrixValue = (Matrix)field!.GetValue(spriteViewMatrix)!;
		matrixValue = Matrix.CreateRotationY(GetValueWithSeed(0, time * 0.25f) * MathHelper.Pi) * matrixValue;
		field.SetValue(spriteViewMatrix, matrixValue);
		*/

		return result;
	}

	public static float GetPowerAtPoint(Vector2 point)
	{
		if (Main.dedServ) {
			return 0f;
		}

		float power = 0f;

		foreach (var shake in EnumerateScreenShakes()) {
			float progress = shake.Progress;

			float intensity;

			if (shake.PowerGradient != null) {
				intensity = MathHelper.Clamp(shake.PowerGradient.GetValue(progress), 0f, 1f);
			} else {
				intensity = MathHelper.Clamp(shake.Power, 0f, 1f);
				intensity *= MathF.Pow(1f - progress, 2f);
			}

			if (shake.Position.HasValue) {
				float distance = Vector2.Distance(shake.Position.Value, point);
				float distanceFactor = 1f - Math.Min(1f, distance / shake.Range);

				intensity *= MathF.Pow(distanceFactor, 2f); // Exponential 
			}

			//power += maxPower * (1f - progress);
			power = MathF.Max(power, intensity);
		}

		return MathHelper.Clamp(power * ScreenShakeStrength.Value, 0f, 1f);
	}

	public static void New(float power, float time, Vector2? position = null, float range = ScreenShake.DefaultRange, string? uniqueId = null)
		=> New(new ScreenShake(power, time, position, range, uniqueId));

	public static void New(Gradient<float> powerGradient, float time, Vector2? position = null, float range = ScreenShake.DefaultRange, string? uniqueId = null)
		=> New(new ScreenShake(powerGradient, time, position, range, uniqueId));

	public static void New(ScreenShake screenShake)
	{
		if (Main.dedServ) {
			return;
		}

		screenShake.Power = Math.Min(screenShake.Power, 1f);
		//screenShake.Length = MathF.Pow(screenShake.Power * 0.6f, 2f);
		screenShake.startTime = TimeSystem.RenderTime;
		screenShake.endTime = screenShake.startTime + screenShake.Length;

		if (screenShake.UniqueId == null) {
			screenShakes.Add(screenShake);
			return;
		}

		int index = screenShakes.FindIndex(s => s.UniqueId == screenShake.UniqueId);

		if (index >= 0) {
			screenShakes[index] = screenShake;
		} else {
			screenShakes.Add(screenShake);
		}
	}

	private static Span<ScreenShake> EnumerateScreenShakes()
	{
		screenShakes.RemoveAll(s => s.TimeLeft <= 0f);

		return CollectionsMarshal.AsSpan(screenShakes);
	}
}
