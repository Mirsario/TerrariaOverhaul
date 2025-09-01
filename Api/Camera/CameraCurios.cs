using System;
using Microsoft.Xna.Framework;
using TerrariaOverhaul.Api.Utilities;
using TerrariaOverhaul.Common.Camera;

namespace TerrariaOverhaul.Api.Camera;

/// <summary> Description used to instance a camera curio. </summary>
public struct CameraCurio()
{
	/// <summary> The string ID that should be used to identify this curio. </summary>
	public required string Identifier { get; set; }
	/// <summary> How strong should the camera be pulled towards this curio. </summary>
	public required float Weight { get; set; }
	/// <summary> How much time in seconds should this curio exist for. </summary>
	public required float LengthInSeconds { get; set; }
	/// <summary> The curio's position in world. </summary>
	public required Vector2 Position { get; set; }
	/// <summary> The curio's active range, including a pow factor. </summary>
	public ExponentialRange? Range { get; set; } = null;
	/// <summary> How fast should the curio's effect begin when it is created, in seconds. </summary>
	public float FadeInLength { get; set; } = 0.5f;
	/// <summary> How fast should the curio's effect stop when it stop existing, in seconds. </summary>
	public float FadeOutLength { get; set; } = 0.5f;
	/// <summary> The target zoom value that this curio should apply, if any. </summary>
	public float? Zoom { get; set; } = null;
	/// <summary> A callback that may be used to update the curio's position. </summary>
	public Func<Vector2?>? Callback { get; set; }
}

/// <summary> Allows creation of camera curios, easier understood as 'focus points'. </summary>
public static class CameraCurios
{
	/// <summary> Creates a new camera curio, or updates an existing one if found using the identifier string. </summary>
	public static void Create(in CameraCurio curio) {
		CameraCuriosImpl.Create(in curio);
	}
}
