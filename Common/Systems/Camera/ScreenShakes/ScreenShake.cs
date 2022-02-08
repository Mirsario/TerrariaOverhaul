using Microsoft.Xna.Framework;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.Systems.Camera.ScreenShakes
{
	public struct ScreenShake
	{
		public const float DefaultRange = 512f;

		public readonly float TimeMax;

		public float power;
		public float time;
		public float range;
		public string uniqueId;
		public Vector2? position;
		public Gradient<float> powerGradient;

		public ScreenShake(Gradient<float> powerGradient, float time, Vector2? position = null, float range = DefaultRange, string uniqueId = null) : this(0f, time, position, range, uniqueId)
		{
			this.powerGradient = powerGradient;
		}

		public ScreenShake(float power, float time, Vector2? position = null, float range = DefaultRange, string uniqueId = null)
		{
			this.power = power;
			this.time = TimeMax = time;
			this.position = position;
			this.range = range;
			this.uniqueId = uniqueId;

			powerGradient = null;
		}
	}
}
