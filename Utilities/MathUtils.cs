namespace TerrariaOverhaul.Utilities
{
	public static class MathUtils
	{
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
	}
}
