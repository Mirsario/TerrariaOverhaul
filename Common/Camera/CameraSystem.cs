using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Camera;

// Static utility properties and core code for other camera systems.

[Autoload(Side = ModSide.Client)]
public sealed class CameraSystem : ModSystem
{
	public delegate void CameraModifierDelegate(Action innerAction);

	private readonly static SortedList<int, CameraModifierDelegate> cameraModifiers = new();

	private static Vector2 lastPositionRemainder;

	public static Vector2 ScreenSize => new(Main.screenWidth, Main.screenHeight);
	public static Vector2 ScreenHalf => new(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
	public static Rectangle ScreenRect => new((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
	public static Vector2 MouseWorld => Main.MouseWorld;
	public static Vector2 ScreenCenter {
		get => new(Main.screenPosition.X + Main.screenWidth * 0.5f, Main.screenPosition.Y + Main.screenHeight * 0.5f);
		set => Main.screenPosition = new Vector2(value.X - Main.screenWidth * 0.5f, value.Y - Main.screenHeight * 0.5f);
	}

	public override void Load()
	{
		// Floor camera position, restoring previous remainders before the next camera update.
		// Maximum priority.
		RegisterCameraModifier(int.MaxValue, innerAction => {
			Main.screenPosition += lastPositionRemainder;

			innerAction();

			//var flooredPosition = new Vector2(MathF.Floor(Main.screenPosition.X * 0.5f), MathF.Floor(Main.screenPosition.Y * 0.5f)) * 2f;
			var flooredPosition = new Vector2(MathF.Floor(Main.screenPosition.X), MathF.Floor(Main.screenPosition.Y));

			flooredPosition += Vector2.One;

			lastPositionRemainder = Main.screenPosition - flooredPosition;

			Main.screenPosition = flooredPosition;
		});

		Main.QueueMainThreadAction(() => {
			On.Terraria.Main.DoDraw_UpdateCameraPosition += orig => {
				if (Main.gameMenu) {
					return;
				}

				int i = 0;
				
				void ModifierRecursion()
				{
					int iCopy = i++;

					if (iCopy < cameraModifiers.Count) {
						cameraModifiers.Values[iCopy](ModifierRecursion);
					} else {
						orig();
					}
				}

				lock (cameraModifiers) {
					ModifierRecursion();
				}
			};
		});
	}

	public override void Unload()
	{
		lock (cameraModifiers) {
			cameraModifiers.Clear();
		}
	}

	public static void RegisterCameraModifier(int priority, CameraModifierDelegate function)
	{
		lock (cameraModifiers) {
			cameraModifiers.Add(-priority, function);
		}
	}
}
