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
	private static float BaseScreenShakePower => 128f;

	public static readonly RangeConfigEntry<float> ScreenShakeStrength = new(ConfigSide.ClientOnly, "Camera", nameof(ScreenShakeStrength), 0f, 1f, () => 1f);

	private static readonly List<ScreenShake> screenShakes = new();

	private static FastNoiseLite? noise;

	public override void Load()
	{
		noise = new FastNoiseLite();

		CameraSystem.RegisterCameraModifier(0, innerAction => {
			innerAction();

			if (Main.gameMenu) {
				return;
			}

			const float BaseScreenShakePower = 256f;

			var samplingPosition = Main.LocalPlayer?.Center ?? CameraSystem.ScreenCenter;
			float screenShakePower = GetPowerAtPoint(samplingPosition);

			screenShakePower = Math.Min(screenShakePower, 1f);

			float trauma = screenShakePower;

			//screenShakePower *= screenShakePower;

			if (InputSystem.GetKey(Microsoft.Xna.Framework.Input.Keys.Tab)) {
				New(2f, 0.5f);
			}

			var noiseOffset = GetNoiseValue();

			Main.screenPosition += noiseOffset * BaseScreenShakePower * screenShakePower;

			// Render debug
			var playerCenter = Main.LocalPlayer!.Center.ToPoint();
			var topLeft = new Point(playerCenter.X - 64, playerCenter.Y - 128);
			int maxHeight = 256;
			int heightA = (int)(maxHeight * trauma);
			int heightB = (int)(maxHeight * screenShakePower);

			DebugSystem.DrawRectangle(new Rectangle(topLeft.X - 32, topLeft.Y, 32, maxHeight), Color.White);
			DebugSystem.DrawRectangle(new Rectangle(topLeft.X - 32, topLeft.Y + maxHeight - heightA, 32, heightA), Color.Green);
			DebugSystem.DrawRectangle(new Rectangle(topLeft.X, topLeft.Y, 32, maxHeight), Color.White);
			DebugSystem.DrawRectangle(new Rectangle(topLeft.X, topLeft.Y + maxHeight - heightB, 32, heightB), Color.Orange);
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

		noise.SetFrequency(5.0f);

		/*
		var result = new Vector2(
			GetValueWithSeed(0, time * 1f),
			GetValueWithSeed(1, time * 1f)
		);
		*/

		float rotation = GetValueWithSeed(0, time * 2f) * MathHelper.Pi;
		//float length = GetValueWithSeed(1, time * 1f);
		var result = new Vector2(1f, 0f).RotatedBy(rotation);

		result = Vector2.Normalize(result) * MathF.Sin(time * 64f);

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
				intensity *= MathF.Pow(1f - progress, 3f);
			}

			if (shake.Position.HasValue) {
				float distance = Vector2.Distance(shake.Position.Value, point);
				float distanceFactor = 1f - Math.Min(1f, distance / shake.Range);

				intensity *= MathF.Pow(distanceFactor, 2f); // Exponential 
			}

			//power += maxPower * (1f - progress);
			power = MathF.Max(power, intensity);
		}

		return MathF.Min(power * ScreenShakeStrength.Value, 1f);
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
