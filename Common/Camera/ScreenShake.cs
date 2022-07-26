using Microsoft.Xna.Framework;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

public struct ScreenShake
{
	public const float DefaultRange = 512f;

	public readonly float TimeMax;

	public float Power;
	public float Time;
	public float Range;
	public string? UniqueId;
	public Vector2? Position;
	public Gradient<float>? PowerGradient;

	public ScreenShake(Gradient<float> powerGradient, float time, Vector2? position = null, float range = DefaultRange, string? uniqueId = null) : this(0f, time, position, range, uniqueId)
	{
		PowerGradient = powerGradient;
	}

	public ScreenShake(float power, float time, Vector2? position = null, float range = DefaultRange, string? uniqueId = null)
	{
		Power = power;
		Time = TimeMax = time;
		Position = position;
		Range = range;
		UniqueId = uniqueId;

		PowerGradient = null;
	}
}
