// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

public struct CameraCurio()
{
	public required float Weight;
	public required float LengthInSeconds;
	public ExponentialRange? Range = null;
	public float FadeInLength = 0.5f;
	public float FadeOutLength = 0.5f;
	public float? Zoom = null;
	public string? UniqueId;
}

[Autoload(Side = ModSide.Client)]
public sealed class CameraCurios : ModSystem
{
	public struct CameraCurioInstance()
	{
		public float StartTime;
		public float EndTime;
		public float Intensity;
		public Vector2 Position;
		public CameraCurio Style;

		public readonly bool Active => TimeSystem.RenderTime >= StartTime && TimeSystem.RenderTime <= EndTime;
	}

	private static readonly List<CameraCurioInstance> curios = new();
	private static WeightedValue<double> lastCalculatedZoom;

	public override void Load()
	{
		CameraSystem.RegisterCameraModifier(-200, ApplyCameraModifier);
	}

	public override void PostUpdateEverything()
	{
		float deltaTime = TimeSystem.LogicDeltaTime;

		foreach (ref var curio in CollectionsMarshal.AsSpan(curios)) {
			float intensityTarget = curio.Active ? 1f : 0f;
			float fadePeriod = curio.Active ? curio.Style.FadeInLength : curio.Style.FadeOutLength;
			curio.Intensity = fadePeriod <= 0f ? intensityTarget : MathUtils.StepTowards(curio.Intensity, intensityTarget, (1f / fadePeriod) * deltaTime);
		}

		curios.RemoveAll(i => !i.Active & i.Intensity <= 0f);
	}

	public override void ModifyTransformMatrix(ref SpriteViewMatrix transform)
	{
		// Apply weighted zoom.
		if (lastCalculatedZoom.TotalWeight > 0f) {
			float offset = (float)lastCalculatedZoom.Total();
			transform.Zoom = new Vector2(
				MathHelper.Clamp(transform.Zoom.X + offset, 1f, 2f),
				MathHelper.Clamp(transform.Zoom.Y + offset, 1f, 2f)
			);
		}
	}

	public static void Create(Vector2 position, CameraCurio style)
	{
		if (Main.dedServ) {
			return;
		}

		CameraCurioInstance instance;
		instance.Style = style;
		instance.Position = position;
		instance.StartTime = TimeSystem.RenderTime;
		instance.EndTime = instance.StartTime + style.LengthInSeconds;
		instance.Intensity = 0f;

		if (style.UniqueId is string uniqueId && curios.FindIndex(i => i.Style.UniqueId == uniqueId) is (>= 0 and int index)) {
			var existing = curios[index];
			curios[index] = instance with {
				Intensity = existing.Intensity,
			};
			return;
		}

		curios.Add(instance);
	}

	private static void ApplyCameraModifier(Action innerAction)
	{
		innerAction();

		var screenCenter = CameraSystem.ScreenCenter;
		var offset = new WeightedValue<Vector2D>(default, 0.0) {
			MinWeight = 1.0,
		};
		var zoom = new WeightedValue<double>(0.0, 0.0) {
			MinWeight = 1.0,
		};

		foreach (ref readonly var curio in CollectionsMarshal.AsSpan(curios)) {
			var targetOffset = curio.Position - screenCenter;
			float intensity = curio.Intensity;

			if (curio.Style.Range.HasValue) {
				intensity *= curio.Style.Range.Value.DistanceFactor(curio.Position.Distance(screenCenter));
			}

			float posWeight = curio.Style.Weight * intensity;
			float zoomWeight = intensity;

			offset.Add(targetOffset.ToF64(), posWeight);

			if (curio.Style.Zoom is { } zoomValue) {
				zoom.Add(zoomValue, zoomWeight);
			}
		}

		if (offset.TotalWeight > 0f) Main.screenPosition += offset.Total().ToF32().ToPoint().ToVector2();
		lastCalculatedZoom = zoom;
	}
}
