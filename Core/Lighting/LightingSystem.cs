// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

#define DEBUG_LIGHTING

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Threading;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Core.Lighting;

// Creates and maintains a small screen-bound buffer containing lighting information.
[Autoload(Side = ModSide.Client)]
public sealed class LightingSystem : ModSystem
{
	private static bool buffersCreated;
	private static bool buffersFilled;
	private static uint lastLightingUpdateCount;
	private static RenderTarget2D? screenSpaceTexture;
	private static RenderTarget2D? tileSpaceTexture;
	private static Surface<Color>? tileSpaceColors;
	private static Vector2 lastCaptureScreenPosition;

	private static int UpdateFrequency => 5;
	private static int ExtraTilesOffset => 4;
	private static int CaptureThreadCount => Math.Min(4, Environment.ProcessorCount / 2);

	public override void Load()
	{
		// This fixes tileTarget not being available in many cases. And other dumb issues.
		MonoModHooks.Add(
			typeof(Main).GetProperty(nameof(Main.RenderTargetsRequired))!.GetMethod!,
			new Func<Func<bool>, bool>(orig => true)
		);

		Main.QueueMainThreadAction(() => {
			Main.OnPreDraw += OnPreDraw;
			IL_Main.DoDraw += DoDrawInjection;
		});
	}
	private static void DoDrawInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);
		il.GotoNext(
			MoveType.After,
			i => i.MatchCall(typeof(Main), "DoDraw_UpdateCameraPosition")
		);
		il.EmitDelegate(PostUpdateCameraPosition);
	}

	public override void Unload()
	{
		Main.OnPreDraw -= OnPreDraw;
		DeinitBuffers();
	}
	public override void PostDrawInterface(SpriteBatch sb)
	{
		DebugLighting(sb);
	}

	private static void OnPreDraw(GameTime obj)
	{
		if (NeedsBufferInit())
			InitBuffers();
	}
	private static void PostUpdateCameraPosition()
	{
		if (NeedsBufferInit())
			InitBuffers();

		if (ShouldCaptureLighting())
			CaptureLighting();

		TransferLighting();
	}

	private static Vector2Int GetBufferSize()
	{
		int offset2 = ExtraTilesOffset + ExtraTilesOffset;
		return new(
			(int)Math.Ceiling(Main.screenWidth / (float)WorldUtils.TileSizeInPixels) + offset2,
			(int)Math.Ceiling(Main.screenHeight / (float)WorldUtils.TileSizeInPixels) + offset2
		);
	}

	private static bool NeedsBufferInit()
		=> !buffersCreated || Main.screenWidth != screenSpaceTexture!.Width || Main.screenHeight != screenSpaceTexture.Height || GetBufferSize() != (Vector2Int)tileSpaceTexture.Size();

	private static void InitBuffers()
	{
		ThreadUtils.RunOnMainThread(static () => {
			var tileSpace = GetBufferSize();
			var screenSpace = new Vector2Int(Main.screenWidth, Main.screenHeight);

			tileSpaceColors?.Dispose();
			tileSpaceTexture?.Dispose();
			screenSpaceTexture?.Dispose();

			tileSpaceColors = new Surface<Color>(tileSpace.X, tileSpace.Y);
			tileSpaceTexture = new RenderTarget2D(Main.graphics.GraphicsDevice, tileSpace.X, tileSpace.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			screenSpaceTexture = new RenderTarget2D(Main.graphics.GraphicsDevice, screenSpace.X, screenSpace.Y, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			// Initialize with data to prevent driver-specific issues.
			TextureUtils.InitializeWithColor(tileSpaceTexture, Color.DarkViolet);
			TextureUtils.InitializeWithColor(screenSpaceTexture, Color.OrangeRed);
			buffersCreated = true;
		});
	}
	private static void DeinitBuffers()
	{
		if (!buffersCreated) return;
		buffersCreated = false;

		if (tileSpaceTexture != null) {
			lock (tileSpaceTexture) {
				var textureHandle = tileSpaceTexture;
				ThreadUtils.RunOnMainThread(textureHandle.Dispose);
				tileSpaceTexture = null;
			}
		}

		tileSpaceColors?.Dispose();
		tileSpaceColors = null;
	}

	private static bool ShouldCaptureLighting()
		=> !buffersFilled || (Main.GameUpdateCount != lastLightingUpdateCount && (Main.GameUpdateCount % UpdateFrequency) == 0);

	// Populates tilespace buffer.
	private unsafe static void CaptureLighting()
	{
		if (!buffersCreated) return;

		int offset1 = ExtraTilesOffset;
		int offset2 = offset1 + offset1;
		var bufferAreaF = new RectFloat(
			(Main.screenPosition.X / WorldUtils.TileSizeInPixels) - offset1,
			(Main.screenPosition.Y / WorldUtils.TileSizeInPixels) - offset1,
			MathF.Ceiling(Main.screenWidth / (float)WorldUtils.TileSizeInPixels) + offset2,
			MathF.Ceiling(Main.screenHeight / (float)WorldUtils.TileSizeInPixels) + offset2
		);
		var bufferArea = new Rectangle(
			(int)bufferAreaF.X,
			(int)bufferAreaF.Y,
			(int)bufferAreaF.Width,
			(int)bufferAreaF.Height
		);
		var bufferPosRemainder = new Vector2(
			bufferAreaF.X - bufferArea.X,
			bufferAreaF.Y - bufferArea.Y
		);

		unsafe {
			var bufferData = tileSpaceColors!.Data;

			FastParallel.For(0, CaptureThreadCount, (int threadId, int numThreads, object context) => {
				int stepsPerThread = bufferData.Length / numThreads;
				int start = threadId * stepsPerThread;
				int end = start + stepsPerThread;
				(int y, int x) = Math.DivRem(start, bufferArea.Width);

				fixed (Color* ptr = tileSpaceColors!.Data) {
					for (int i = start; i < end; i++) {
						ptr[i] = Terraria.Lighting.GetColor(bufferArea.X + x, bufferArea.Y + y);

						x += 1;
						if (x == bufferArea.Width) {
							x = 0;
							y += 1;
						}
					}
				}
			});

			//fixed (Color* ptr = tileSpaceColors!.Data) {
			//	int i = 0;
			//	for (int y = 0; y < bufferArea.Height; y++) {
			//		for (int x = 0; x < bufferArea.Width; x++) {
			//			ptr[i] = Terraria.Lighting.GetColor(bufferArea.X + x, bufferArea.Y + y);
			//			i += 1;
			//		}
			//	}
			//}

			lock (tileSpaceTexture!) tileSpaceTexture.SetData(bufferData);
		}

		buffersFilled = true;
		lastLightingUpdateCount = Main.GameUpdateCount;
		lastCaptureScreenPosition = CameraSystem.ScreenCenter - (bufferPosRemainder * WorldUtils.TileSizeInPixels);
	}

	public static bool TryGetLightingBuffer([NotNullWhen(true)] out Texture2D? result)
	{
		if (buffersCreated && buffersFilled && screenSpaceTexture is { } lighting) {
			result = lighting;
			return true;
		}

		result = default;
		return false;
	}
	// Renders tilespace capture onto the screenspace buffer.
	private static void TransferLighting()
	{
		var sb = Main.spriteBatch;
		var graphicsDevice = Main.graphics.GraphicsDevice;

		graphicsDevice.SetRenderTarget(screenSpaceTexture);
		//graphicsDevice.Clear(Color.DarkViolet);

		var screenSpaceTargetSize = new Vector2Int(
			screenSpaceTexture!.Width,
			screenSpaceTexture!.Height
		);
		var tileSpaceTargetSizeInPixels = new Vector2Int(
			tileSpaceTexture!.Width * WorldUtils.TileSizeInPixels,
			tileSpaceTexture!.Height * WorldUtils.TileSizeInPixels
		);
		var moveOffset = lastCaptureScreenPosition - CameraSystem.ScreenCenter;
		var diffOffset = -(tileSpaceTargetSizeInPixels - screenSpaceTargetSize) / 2;
		var totalOffset = moveOffset + diffOffset;

		Rectangle dstRect;
		dstRect.X = (int)totalOffset.X;
		dstRect.Y = (int)totalOffset.Y;
		dstRect.Width = tileSpaceTargetSizeInPixels.X;
		dstRect.Height = tileSpaceTargetSizeInPixels.Y;

		sb.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);
		sb.Draw(tileSpaceTexture, dstRect, Color.White);
		sb.End();

		graphicsDevice.SetRenderTarget(null);
	}

	private static void DebugLighting(SpriteBatch sb)
	{
#if DEBUG_LIGHTING
		if (Input.InputSystem.GetKey(Microsoft.Xna.Framework.Input.Keys.L)) {
			var dstA = new Rectangle(32, Main.screenHeight / 2 - 64, Main.screenWidth / 2 - 64, Main.screenHeight / 2 - 64);
			var dstB = new Rectangle(Main.screenWidth / 2 + 32, Main.screenHeight / 2 - 64, Main.screenWidth / 2 - 64, Main.screenHeight / 2 - 64);

			sb.Draw(screenSpaceTexture, dstA, Color.White);
			sb.Draw(tileSpaceTexture, dstB, Color.White);
			Debugging.DebugSystem.DrawRectangle(dstA, Color.Yellow, width: 4);
			Debugging.DebugSystem.DrawRectangle(dstB, Color.Beige, width: 4);
		}
		if (Input.InputSystem.GetKey(Microsoft.Xna.Framework.Input.Keys.J)) {
			var src = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
			var dst = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
			sb.Draw(screenSpaceTexture, dst, src, Color.White.WithAlpha(192));
		}
#endif
	}
}
