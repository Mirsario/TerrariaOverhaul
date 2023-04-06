#define OUTPUT_TEST

using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Utilities;

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

	private static DrawnPart currentlyDrawnPart;

	public override void Load()
	{
		On.Terraria.Player.KillMe += KillMeDetour;
	}

	public override void HideDrawLayers(PlayerDrawSet drawInfo)
	{
		if (currentlyDrawnPart == DrawnPart.None) {
			return;
		}

		var drawnPart = currentlyDrawnPart;
		var layers = PlayerDrawLayerLoader.DrawOrder;

		bool ShouldShowLayer(PlayerDrawLayer layer) => drawnPart switch {
			DrawnPart.Head => layer == PlayerDrawLayers.Head,
			DrawnPart.Torso => layer == PlayerDrawLayers.Torso,
			DrawnPart.Arms => layer == PlayerDrawLayers.ArmOverItem,
			DrawnPart.Legs => layer == PlayerDrawLayers.Leggings,
			_ => throw new NotImplementedException(),
		};

		foreach (var layer in layers) {
			if (!ShouldShowLayer(layer)) {
				layer.Hide();
			}
		}
	}

	// Up for detouring.
	[MethodImpl(MethodImplOptions.NoInlining)]
	public static Color GetPlayerBloodColor(Player player)
		=> Color.DarkRed;

	private static void KillMeDetour(On.Terraria.Player.orig_KillMe orig, Player player, PlayerDeathReason damageSource, double damage, int hitDirection, bool pvp)
	{
		orig(player, damageSource, damage, hitDirection, pvp);

		if (!ChildSafety.Disabled) {
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

		for (int i = 0; i < framing.ColumnCount; i++) {
			var gore = Main.gore[DynamicGore.NewGore(texture, source, player.Center, GetGoreVelocity(player))];

			gore.drawOffset = new Vector2(0f, 6f);
			gore.Frame = framing with {
				CurrentColumn = (byte)i,
			};

			if (gore is OverhaulGore oGore) {
				oGore.BleedColor = bloodColor;
			}
		}

		// Generic gore
		for (int i = 0; i < 7; i++) {
			var gore = Gore.NewGoreDirect(source, player.Center, GetGoreVelocity(player), ModContent.GoreType<GenericGore>());

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
		
		// Overrides necessary for successful rendering
		using var fullBrightOverride = new ValueOverride<bool>(typeof(Main), nameof(Main.gameMenu), true);
		using var screenPositionOverride = new ValueOverride<Vector2>(typeof(Main), nameof(Main.screenPosition), default);
		using var rotationOriginOverride = new ValueOverride<Vector2>(typeof(Player), nameof(Terraria.Player.fullRotationOrigin), player, default);
		using var headRotationOverride = new ValueOverride<float>(typeof(Player), nameof(Terraria.Player.headRotation), player, default);
		using var alphaOverride = new ValueOverride<int>(typeof(Player), nameof(Terraria.Player.immuneAlpha), player, 0);

		// Prepare for rendering

		var oldRenderTargets = graphicsDevice.GetRenderTargets();

		graphicsDevice.SetRenderTarget(renderTarget);
		graphicsDevice.Clear(Color.Transparent);
		spriteBatch.Begin();

		var defaultFrame = PlayerFrames.Idle.ToRectangle();

		// Draw body parts

		void RenderPart(DrawnPart part, Vector2 position, float rotation = 0f, Rectangle? headFrame = null, Rectangle? bodyFrame = null, Rectangle? legsFrame = null)
		{
			currentlyDrawnPart = part;

			player.direction = 1;
			player.headFrame = headFrame ?? defaultFrame;
			player.bodyFrame = bodyFrame ?? defaultFrame;
			player.legFrame = legsFrame ?? defaultFrame;

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
