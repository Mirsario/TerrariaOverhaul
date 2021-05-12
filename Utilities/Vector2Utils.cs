using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities
{
	public static class Vector2Utils
	{
		public static Vector2 StepTowards(Vector2 value, Vector2 goal, float step) => StepTowards(value, goal, new Vector2(step, step));
		public static Vector2 StepTowards(Vector2 value, Vector2 goal, Vector2 step) => new(
			MathUtils.StepTowards(value.X, goal.X, step.X),
			MathUtils.StepTowards(value.Y, goal.Y, step.Y)
		);

		public static Vector2 Round(Vector2 value) => new Vector2((float)Math.Round(value.X), (float)Math.Round(value.Y));
		public static Vector2 Floor(Vector2 value) => new Vector2((float)Math.Floor(value.X), (float)Math.Floor(value.Y));
		public static Vector2 Ceiling(Vector2 value) => new Vector2((float)Math.Ceiling(value.X), (float)Math.Ceiling(value.Y));
	}
}
