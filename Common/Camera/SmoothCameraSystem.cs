// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Camera;

[Autoload(Side = ModSide.Client)]
internal sealed class SmoothCameraSystem : ModSystem
{
	//public static readonly ConfigEntry<bool> SmoothCamera = new(ConfigSide.ClientOnly, true, "Camera");
	public static readonly RangeConfigEntry<float> CameraSmoothness = new(ConfigSide.ClientOnly, 1f, (0f, 1f), "Camera");

	// The reason this isn't taken at the start of the modifier is because in that case this modifier will smooth out higher
	// priority modifications, like screenshake.
	private static Vector2? oldPosition;

	public override void Load()
	{
		CameraSystem.RegisterCameraModifier(-100, innerAction => {
			//var oldPosition = Main.screenPosition;

			oldPosition ??= Main.screenPosition;

			innerAction();

			var newPosition = Main.screenPosition;
			var difference = newPosition - oldPosition.Value;
			float differenceLength = difference.SafeLength();
			float maxDifferenceLength = new Vector2(Main.screenWidth, Main.screenHeight).SafeLength() * 0.5f + 100f;

#if DEBUG
			/*
			CameraSmoothness.Value = InputSystem.GetKey(Keys.K) ? 0f : 1f;
			*/
#endif

			if (CameraSmoothness > 0f && differenceLength < maxDifferenceLength) {
				const float BaseSmoothness = 0.1f;

				float smoothness = BaseSmoothness * CameraSmoothness;
				float deltaTime = CameraSystem.LimitCameraUpdateRate ? TimeSystem.LogicDeltaTime : TimeSystem.RenderDeltaTime;

				Main.screenPosition = MathUtils.Damp(oldPosition.Value, newPosition, smoothness * smoothness, deltaTime);
			}

			oldPosition = Main.screenPosition;
		});
	}
}
