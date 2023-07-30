namespace TerrariaOverhaul.Common.Camera;

public struct ScreenShake
{
	public delegate float PowerDelegate(float progress);

	public const float DefaultRange = 512f;

	public float Power = 0.5f;
	public PowerDelegate? PowerFunction;
	public float LengthInSeconds = 0.5f;
	public float Range = DefaultRange;
	public string? UniqueId;

	public ScreenShake(PowerDelegate powerFunction, float lengthInSeconds) : this()
	{
		PowerFunction = powerFunction;
		LengthInSeconds = lengthInSeconds;
	}

	public ScreenShake(float power, float lengthInSeconds) : this()
	{
		Power = power;
		LengthInSeconds = lengthInSeconds;
	}
}
