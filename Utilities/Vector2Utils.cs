using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities
{
	public static class Vector2Utils
	{
		public static Vector2 StepTowards(Vector2 value, Vector2 goal, float step) => StepTowards(value, goal, new Vector2(step, step));
		public static Vector2 StepTowards(Vector2 value, Vector2 goal, Vector2 step) => new Vector2(
			MathUtils.StepTowards(value.X, goal.X, step.X),
			MathUtils.StepTowards(value.Y, goal.Y, step.Y)
		);
	}
}
