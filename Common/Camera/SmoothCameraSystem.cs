using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.Camera;

[Autoload(Side = ModSide.Client)]
public sealed class SmoothCameraSystem : ModSystem
{
	public static readonly ConfigEntry<bool> SmoothCamera = new(ConfigSide.ClientOnly, "Camera", nameof(SmoothCamera), () => true);

	public override void Load()
	{
		CameraSystem.RegisterCameraModifier(100, innerAction => {
			var oldPosition = Main.screenPosition;

			innerAction();

			var newPosition = Main.screenPosition;

			Main.screenPosition = Damp(oldPosition, newPosition, 0.025f, TimeSystem.RenderDeltaTime);
		});
	}

	public static float Damp(float source, float destination, float smoothing, float dt)
	{
		// See this:
		// https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp

		return MathHelper.Lerp(source, destination, 1f - MathF.Pow(smoothing, dt));
	}

	public static Vector2 Damp(Vector2 source, Vector2 destination, float smoothing, float dt) => new(
		Damp(source.X, destination.X, smoothing, dt),
		Damp(source.Y, destination.Y, smoothing, dt)
	);
}
