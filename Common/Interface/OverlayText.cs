// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Interface;

internal struct OverlayTextLine()
{
	public required float AnimationLength;
	public required string Text;
	public required (Vector2 Percent, Vector2 Pixels) Position;
	public Vector2 Scale = Vector2.One;
	public Color PrimaryColor = Color.White;
	public Color OutlineColor = Color.Black;
	public Asset<DynamicSpriteFont>? FontOverride;
	public (float Frequency, Vector2 Strength)? ShakeEffect;
	public (float Start, float End)? FadeInEffect = (0.0f, 0.1f);
	public (float Start, float End)? FadeOutEffect = (0.9f, 1.0f);
	public (float Start, float End, Vector2 Scale)? ScaleInEffect = null;
	public (float Start, float End, Vector2 Scale)? ScaleOutEffect = null;
}

internal sealed class OverlayText : ModSystem
{
	private struct Instance()
	{
		public required OverlayTextLine Style;
		public bool JustStarted = true;
		public float StartTime = float.NegativeInfinity;
		public float EndTime = float.NegativeInfinity;
	}

	private static readonly LegacyGameInterfaceLayer interfaceLayer = new($"{nameof(TerrariaOverhaul)}/TextOverlays", InterfaceLayer, InterfaceScaleType.UI);
	private static readonly List<Instance> activeLines = new();
	private static FastNoiseLite? noise;

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		layers.Add(interfaceLayer);
	}

	public static void Create(in OverlayTextLine description)
	{
		activeLines.Add(new Instance { Style = description });
	}

	private static void Update(ref Instance instance)
	{
		if (instance.JustStarted) {
			instance.StartTime = TimeSystem.RenderTime;
			instance.EndTime = instance.StartTime + instance.Style.AnimationLength;
			instance.JustStarted = false;
		}
	}

	private static bool InterfaceLayer()
	{
		var sb = Main.spriteBatch;
		float time = TimeSystem.RenderTime;

		foreach (ref var instance in CollectionsMarshal.AsSpan(activeLines)) {
			Update(ref instance);

			ref readonly var style = ref instance.Style;
			float progress = MathUtils.Clamp01((time - instance.StartTime) / (instance.EndTime - instance.StartTime));

			var opacity = 1f;
			if (style.FadeInEffect is { } fadeIn)
				opacity *= Easings.EaseInSquare(Easings.FadeIn(progress, fadeIn.Start, fadeIn.End));
			if (style.FadeOutEffect is { } fadeOut)
				opacity *= Easings.EaseInSquare(Easings.FadeOut(progress, fadeOut.Start, fadeOut.End));

			var scale = style.Scale;
			if (style.ScaleInEffect is { } scaleIn)
				scale = Vector2.Lerp(scaleIn.Scale, scale, Easings.EaseInSquare(Easings.FadeIn(progress, scaleIn.Start, scaleIn.End)));
			if (style.ScaleOutEffect is { } scaleOut)
				scale = Vector2.Lerp(scaleOut.Scale, scale, Easings.EaseInSquare(Easings.FadeOut(progress, scaleOut.Start, scaleOut.End)));

			var position = (new Vector2(Main.screenWidth, Main.screenHeight) * style.Position.Percent) + style.Position.Pixels;
			if (style.ShakeEffect is { } shake)
				position += shake.Strength * SampleNoise(time, shake.Frequency, position);

			var font = (style.FontOverride ?? FontAssets.DeathText).Value;
			var textSize = font.MeasureString(style.Text);
			var origin = textSize * 0.5f;

			byte a = (byte)(opacity * byte.MaxValue);
			var alphaColor = new Color(a, a, a, a);
			var primaryColor = style.PrimaryColor.MultiplyRGBA(alphaColor);
			var outlineColor = style.OutlineColor.MultiplyRGBA(alphaColor);

			sb.DrawStringOutlined(font, style.Text, position, primaryColor, origin, scale, outlineColor);
		}

		activeLines.RemoveAll(i => time >= i.EndTime);

		return true;
	}

	private static Vector2 SampleNoise(float time, float frequency, Vector2 offset)
	{
		noise ??= new FastNoiseLite();
		noise.SetFrequency(frequency);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		return new(noise!.GetNoise(time, offset.X), noise!.GetNoise(time, offset.Y));
	}
}
