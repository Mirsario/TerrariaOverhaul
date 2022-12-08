using System;
using Microsoft.Xna.Framework;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

public struct ScreenShake
{
	public const float DefaultRange = 512f;

	internal float startTime = 0f;
	internal float endTime = 0f;

	public float Power;
	public float Length;
	public float Range;
	public string? UniqueId;
	public Vector2? Position;
	public Gradient<float>? PowerGradient;

	public float TimeLeft => MathF.Max(0f, endTime - TimeSystem.RenderTime);
	public float Progress => Length > 0f ? MathHelper.Clamp((TimeSystem.RenderTime - startTime) / Length, 0f, 1f) : 1f;

	public ScreenShake(Gradient<float> powerGradient, float lengthInSeconds, Vector2? position = null, float range = DefaultRange, string? uniqueId = null) : this(0f, lengthInSeconds, position, range, uniqueId)
	{
		PowerGradient = powerGradient;
	}

	public ScreenShake(float power, float lengthInSeconds, Vector2? position = null, float range = DefaultRange, string? uniqueId = null)
	{
		Power = power;
		Length = lengthInSeconds;
		Position = position;
		Range = range;
		UniqueId = uniqueId;

		PowerGradient = null;
	}
}
