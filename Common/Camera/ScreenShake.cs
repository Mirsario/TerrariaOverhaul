using Microsoft.Xna.Framework;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

public struct ScreenShake
{
	public const float DefaultRange = 512f;

	public float Power = 0.5f;
	public Gradient<float>? PowerGradient;
	public float LengthInSeconds = 0.5f;
	public float Range = DefaultRange;
	public string? UniqueId;

	public ScreenShake(Gradient<float> powerGradient, float lengthInSeconds) : this()
	{
		PowerGradient = powerGradient;
		LengthInSeconds = lengthInSeconds;
	}

	public ScreenShake(float power, float lengthInSeconds) : this()
	{
		Power = power;
		LengthInSeconds = lengthInSeconds;
	}
}
