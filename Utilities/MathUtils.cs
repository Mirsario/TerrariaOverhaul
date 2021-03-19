using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities
{
	public static class MathUtils
	{
		public static float RadiansToPitch(float radians)
		{
			radians = Modulo(radians, MathHelper.TwoPi);

			if(radians < MathHelper.PiOver2) {
				return 0.5f - radians / MathHelper.Pi; // [0.5 - 1.0]
			} else if(radians < MathHelper.Pi * 1.5f) {
				return (radians - MathHelper.PiOver2) / MathHelper.Pi; // [0.0 - 1.0]
			} else {
				return 1f - ((radians - MathHelper.Pi * 1.5f) / MathHelper.Pi); // [0.0 - 0.5]
			}
		}
		public static float DegreesToPitch(float degrees)
		{
			degrees = Modulo(degrees, 360f);

			if(degrees < 90f) {
				return 0.5f - (degrees / 180f); // [0.5 - 1.0]
			} else if(degrees < 270f) {
				return (degrees - 90f) / 180f; // [0.0 - 1.0]
			} else {
				return 1f - ((degrees - 270f) / 180f); // [0.0 - 0.5]
			}
		}

		public static int Modulo(int value, int length)
		{
			int r = value % length;

			return r < 0 ? r + length : r;
		}
		public static float Modulo(float value, float length) => value - (float)Math.Floor(value / length) * length;
		public static double Modulo(double value, double length) => value - (float)Math.Floor(value / length) * length;

		public static int Clamp(int value, int min, int max) => value <= min ? min : (value >= max ? max : value);
		public static float Clamp(float value, float min, float max) => value <= min ? min : (value >= max ? max : value);
		public static float Clamp01(float value) => value <= 0f ? 0f : (value >= 1f ? 1f : value);

		public static int MaxAbs(int a, int b) => Math.Abs(a) >= Math.Abs(b) ? a : b;
		public static float MaxAbs(float a, float b) => Math.Abs(a) >= Math.Abs(b) ? a : b;

		public static int MinAbs(int a, int b) => Math.Abs(a) <= Math.Abs(b) ? a : b;
		public static float MinAbs(float a, float b) => Math.Abs(a) <= Math.Abs(b) ? a : b;

		public static float InverseLerp(float value, float start, float end) => (value - start) / (end - start);

		public static float StepTowards(float value, float goal, float step)
		{
			if(goal > value) {
				value += step;

				if(value > goal) {
					return goal;
				}
			} else if(goal < value) {
				value -= step;

				if(value < goal) {
					return goal;
				}
			}

			return value;
		}

		public static float DistancePower(float distance, float maxDistance)
		{
			if(distance > maxDistance) {
				return 0f;
			}

			if(distance <= 0f) {
				return 1f;
			}

			float result = 1f - distance / maxDistance;

			if(float.IsNaN(result)) {
				result = 0f;
			}

			return result;
		}
	}
}
