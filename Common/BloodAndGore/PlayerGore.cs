// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

//#define OUTPUT_TEST

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;

#pragma warning disable IDE0060 // Remove unused parameter

namespace TerrariaOverhaul.Common.ModEntities.Players;

[Autoload(Side = ModSide.Client)]
public sealed class PlayerGore : ModPlayer
{
	private enum DrawnPart
	{
		None,
		Head,
		Torso,
		Arms,
		Legs,
	}

	public static readonly ConfigEntry<bool> EnablePlayerCharacterGore = new(ConfigSide.ClientOnly, true, "BloodAndGore");
	public static readonly ConfigEntry<bool> FocusCameraOnPlayerDeath = new(ConfigSide.ClientOnly, true, "Camera");

	private static DrawnPart currentlyDrawnPart;
	private static List<(int Index, int Type, float Weight, Vector2 Position)>? lastDeathGore;

	public override void Load()
	{
		On_Player.KillMe += KillMeDetour;
	}

	public override void HideDrawLayers(PlayerDrawSet drawInfo)
	{
		if (currentlyDrawnPart == DrawnPart.None) {
			return;
		}

		var drawnPart = currentlyDrawnPart;
		var layers = PlayerDrawLayerLoader.DrawOrder;

		bool ShouldShowLayer(PlayerDrawLayer layer) => drawnPart switch {
			DrawnPart.Head => layer.IsHeadLayer,
			DrawnPart.Torso => layer == PlayerDrawLayers.Torso || layer == PlayerDrawLayers.FrontAccFront || layer == PlayerDrawLayers.FrontAccBack || layer == PlayerDrawLayers.NeckAcc,
			DrawnPart.Arms => layer == PlayerDrawLayers.ArmOverItem || layer == PlayerDrawLayers.HandOnAcc || layer == PlayerDrawLayers.OffhandAcc,
			DrawnPart.Legs => layer == PlayerDrawLayers.Leggings || layer == PlayerDrawLayers.WaistAcc,
			_ => throw new NotImplementedException(),
		};

		foreach (var layer in layers) {
			if (!ShouldShowLayer(layer)) {
				layer.Hide();
			}
		}
	}

	public override void UpdateDead()
	{
		if (!Player.IsLocal()) return;

		var focusPoint = Main.LocalPlayer.Center;
		if (lastDeathGore is { Count: > 0 } list) {
			var weightedPosition = new WeightedValue<Vector2D>(default, 0.0);
			foreach (ref var gore in CollectionsMarshal.AsSpan(list)) {
				if (gore.Index > 0 && Main.gore[gore.Index] is { active: true } inst && gore.Type == inst.type)
					gore.Position = inst.AABBRectangle.Center();
				else
					gore.Index = -1;

				weightedPosition.Add(gore.Position.ToF64(), gore.Weight);
			}

			focusPoint = weightedPosition.Total().ToF32();
		}

		CameraCurios.Create(focusPoint, new CameraCurio {
			UniqueId = "PlayerDeath",
			LengthInSeconds = 0.05f,
			Range = null,
			Weight = 1f,
			FadeOutLength = 0.1f,
			Zoom = +0.5f,
		});
	}

	// Up for detouring.
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Color GetPlayerBloodColor(Player player)
		=> Color.DarkRed;

	private static void KillMeDetour(On_Player.orig_KillMe orig, Player player, PlayerDeathReason damageSource, double damage, int hitDirection, bool pvp)
	{
		orig(player, damageSource, damage, hitDirection, pvp);

		if (!EnablePlayerCharacterGore || !ChildSafety.Disabled) {
			return;
		}

		// Remove vanilla "gore"
		player.fullRotation = default;
		player.immuneAlpha = 255;
		(player.headPosition, player.bodyPosition, player.legPosition) = (default, default, default);
		(player.headVelocity, player.bodyVelocity, player.legVelocity) = (default, default, default);
		(player.headRotation, player.bodyRotation, player.legRotation) = (default, default, default);

		// Spawn head
		Main.QueueMainThreadAction(() => CreateGores(player));
	}

	private static void CreateGores(Player player)
	{
		static Vector2 GetGoreVelocity(Player player)
			=> player.velocity * 0.5f + Main.rand.NextVector2Circular(1f, 1f);

		var source = player.GetSource_FromThis();
		var bloodColor = GetPlayerBloodColor(player);

		// Generated gore
		var texture = CreateGoreTexture(player, bloodColor, out var framing);

		if (player.IsLocal()) {
			lastDeathGore ??= new();
			lastDeathGore.Clear();
		}

		for (int i = 0; i < framing.ColumnCount; i++) {
			var offset = i switch {
				0 => new Vector2(+00.0f, -20.0f), // Head
				2 => new Vector2(-10.0f, +00.0f), // Left arm
				3 => new Vector2(+10.0f, +00.0f), // Right arm
				4 => new Vector2(-10.0f, +20.0f), // Left leg
				5 => new Vector2(+10.0f, +20.0f), // Right leg
				_ => default, // Torso
			};
			float curioWeight = i switch {
				0 => 3f,
				1 => 1f,
				_ => 0.2f,
			};

			var position = player.Center + offset;
			int goreIndex = DynamicGore.NewGore(texture, source, position, GetGoreVelocity(player));
			var gore = Main.gore[goreIndex];

			gore.drawOffset = new Vector2(0f, 8f);
			gore.Frame = framing with {
				CurrentColumn = (byte)i,
			};

			if (gore is OverhaulGore oGore) {
				oGore.BleedColor = bloodColor;
			}

			if (player.IsLocal()) {
				lastDeathGore!.Add((goreIndex, DynamicGore.Type, curioWeight, position));
			}
		}

		// Generic gore
		for (int i = 0; i < 7; i++) {
			var position = player.Center + Main.rand.NextVector2Circular(20f, 28f);
			var gore = Gore.NewGoreDirect(source, position, GetGoreVelocity(player), ModContent.GoreType<GenericGore>());

			if (gore is OverhaulGore oGore) {
				oGore.BleedColor = bloodColor;
			}
		}
	}

	private static DynamicGore.TextureHandle CreateGoreTexture(Player player, Color bloodColor, out SpriteFrame framing)
	{
		// Framing

		const int NumBodyParts = 6;
		const int NumVariants = 4;
		const int CellSize = 32;

		framing = new SpriteFrame(NumBodyParts, 1) {
			PaddingX = 0,
			PaddingY = 0,
		};

		// Create texture

		var spriteBatch = Main.spriteBatch;
		var graphicsDevice = Main.graphics.GraphicsDevice;
		var renderTarget = new RenderTarget2D(graphicsDevice, framing.ColumnCount * CellSize, framing.RowCount * CellSize);
		var camera = Main.Camera;

		// Overrides necessary for successful rendering
		using var fullBrightOverride = ValueOverride.Create(ref Main.gameMenu, true);
		using var screenPositionOverride = ValueOverride.Create(ref Main.screenPosition, default);
		using var rotationOriginOverride = ValueOverride.Create(ref player.fullRotationOrigin, default);
		using var headRotationOverride = ValueOverride.Create(ref player.headRotation, default);
		using var alphaOverride = ValueOverride.Create(ref player.immuneAlpha, 0);

		// Prepare for rendering

		var oldRenderTargets = graphicsDevice.GetRenderTargets();

		graphicsDevice.SetRenderTarget(renderTarget);
		graphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, camera.Sampler, DepthStencilState.None, camera.Rasterizer, null);

		var defaultFrame = PlayerFrames.Idle.ToRectangle();

		// Draw body parts

		void RenderPart(DrawnPart part, Vector2 position, float rotation = 0f, Rectangle? headFrame = null, Rectangle? bodyFrame = null, Rectangle? legsFrame = null)
		{
			currentlyDrawnPart = part;

			player.direction = 1;
			player.headFrame = headFrame ?? defaultFrame;
			player.bodyFrame = bodyFrame ?? defaultFrame;
			player.legFrame = legsFrame ?? defaultFrame;

			bool wearsRobe = player.wearsRobe && part != DrawnPart.Legs;
			using var robeOverride = ValueOverride.Create(ref player.wearsRobe, wearsRobe);

			Main.PlayerRenderer.DrawPlayer(Main.Camera, player, position, rotation, default);
		}

		RenderPart(DrawnPart.Head, new Vector2(0f, 0f) + new Vector2(6f, 8f), headFrame: PlayerFrames.Idle.ToRectangle());
		RenderPart(DrawnPart.Torso, new Vector2(32f, 0f) + new Vector2(6f, -8f), bodyFrame: PlayerFrames.Jump.ToRectangle());
		RenderPart(DrawnPart.Arms, new Vector2(64f, 0f) + new Vector2(14f, 0f), bodyFrame: PlayerFrames.Use1.ToRectangle());
		RenderPart(DrawnPart.Arms, new Vector2(96f, 0f) + new Vector2(6f, -6f), bodyFrame: PlayerFrames.Use3.ToRectangle());
		RenderPart(DrawnPart.Legs, new Vector2(128f, 0f) + new Vector2(3f, -22f), legsFrame: new Rectangle(0, 280, 22, 56));
		RenderPart(DrawnPart.Legs, new Vector2(160f, 0f) + new Vector2(8f, -20f), legsFrame: new Rectangle(20, 280, 20, 56));

		// Overlay blood

		var overlayTexture = ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/DynamicGore/PlayerGoreOverlays", AssetRequestMode.ImmediateLoad).Value;
		var overlayFraming = new SpriteFrame(NumBodyParts, NumVariants) {
			PaddingX = 0,
			PaddingY = 0,
		};

		for (int i = 0; i < NumBodyParts; i++) {
			var srcFrame = overlayFraming with {
				CurrentColumn = (byte)i,
				CurrentRow = (byte)Main.rand.Next(overlayFraming.RowCount)
			};
			var dstFrame = framing with {
				CurrentColumn = (byte)i,
			};

			var srcRect = srcFrame.GetSourceRectangle(overlayTexture);
			var dstRect = dstFrame.GetSourceRectangle(renderTarget);

			spriteBatch.Draw(overlayTexture, dstRect, srcRect, bloodColor);
		}

		// Finish rendering
		spriteBatch.End();
		graphicsDevice.SetRenderTargets(oldRenderTargets);

		// Cleanup
		currentlyDrawnPart = DrawnPart.None;
		player.headFrame = defaultFrame;
		player.bodyFrame = defaultFrame;
		player.legFrame = defaultFrame;

		// Save the texture
#if OUTPUT_TEST
		using var fileStream = System.IO.File.OpenWrite("test.png");

		renderTarget.SaveAsPng(fileStream, renderTarget.Width, renderTarget.Height);
#endif

		// Return a texture handle
		var textureHandle = DynamicGore.RegisterTexture(renderTarget, autoRemove: true, disposeOnRemoval: true);

		return textureHandle;
	}
}
