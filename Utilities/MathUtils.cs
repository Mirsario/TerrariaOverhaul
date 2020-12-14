using System;

namespace TerrariaOverhaul.Utilities
{
	public static class MathUtils
	{
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
