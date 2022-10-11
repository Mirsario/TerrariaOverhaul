using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.Camera;

public sealed class SmoothCameraSystem : ModSystem
{
	public static readonly ConfigEntry<bool> SmoothCamera = new(ConfigSide.ClientOnly, "Camera", nameof(SmoothCamera), () => true);

	private static Vector2 lastPositionRemainder;

	public override void Load()
	{
		On.Terraria.Main.DoDraw_UpdateCameraPosition += orig => {
			var oldPosition = Main.screenPosition + lastPositionRemainder;

			orig();

			var newPosition = Main.screenPosition;

			Main.screenPosition = Damp(oldPosition, newPosition, 0.025f, TimeSystem.RenderDeltaTime);

			var flooredPosition = new Vector2(MathF.Floor(Main.screenPosition.X), MathF.Floor(Main.screenPosition.Y));

			lastPositionRemainder = Main.screenPosition - flooredPosition;

			Main.screenPosition = flooredPosition;
		};
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
