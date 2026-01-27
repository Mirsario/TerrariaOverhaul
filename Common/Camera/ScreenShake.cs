// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

namespace TerrariaOverhaul.Common.Camera;

internal struct ScreenShake
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
