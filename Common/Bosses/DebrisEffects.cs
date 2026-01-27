// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Bosses;

internal sealed class DebrisEffects : ModSystem
{
	public struct Style
	{
		public SoundStyle Sound = DefaultSound;
		public float RangeInPixels = 48f;
		public float LengthInSeconds = 0.33f;

		public Style() { }
	}

	public struct Instance
	{
		public Style Style;
		public SlotId Sound;
		public Vector2 Position;
		public float StartTime;
		public float EndTime;

		public readonly float TimeLeft => MathF.Max(0f, EndTime - TimeSystem.RenderTime);
		public readonly float Progress => Style.LengthInSeconds > 0f ? MathHelper.Clamp((TimeSystem.RenderTime - StartTime) / Style.LengthInSeconds, 0f, 1f) : 1f;
		
		public Instance() { }
	}

	public static readonly SoundStyle DefaultSound = new() {
		SoundPath = $"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/DebrisLoopViolent",
		IsLooped = true,
		Volume = 0.625f,
		PitchVariance = 0.25f,
		MaxInstances = 3,
	};

	public static List<Instance> DebrisInstances { get; } = [];

	public override void Load()
	{
		TileSoundOcclusion.SetEnabledForSoundStyle(DefaultSound, false);
	}

	public override void PostUpdateEverything()
	{
		const float ParticlesDistance = 2048f;
		const float ParticlesDistanceSqr = ParticlesDistance * ParticlesDistance;

		float time = TimeSystem.LogicTime;

		foreach (ref var debris in CollectionsMarshal.AsSpan(DebrisInstances)) {
			bool collect = time >= debris.EndTime;
			bool visible = CameraSystem.ScreenCenter.DistanceSQ(debris.Position) <= ParticlesDistanceSqr;

			// Spawn tile dust.
			if (visible) {
				var worldRect = new Rectangle((int)debris.Position.X, (int)debris.Position.Y, 0, 0).Extended((int)debris.Style.RangeInPixels);

				if (Main.GameUpdateCount % 2 == 0) {
					SpawnTileDust(worldRect);
				}
			}

			bool startedSound = false;
			Recheck:
			if (SoundEngine.TryGetActiveSound(debris.Sound, out var sound)) {
				if (collect) {
					sound.Stop();
				} else {
					sound.Volume = 1f - debris.Progress;
					sound.Position = debris.Position;
				}
			} else if (!collect && !startedSound) {
				debris.Sound = SoundEngine.PlaySound(
					debris.Style.Sound with { IsLooped = true, SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest },
					debris.Position
				);
				startedSound = true;
				goto Recheck;
			}
		}

		// Cleanup.
		DebrisInstances.RemoveAll(i => time >= i.EndTime);
	}

	public static void SpawnTileDust(Rectangle rectangle)
	{
		int xStart = (rectangle.X / WorldUtils.TileSizeInPixels) - 1;
		int yStart = (rectangle.Y / WorldUtils.TileSizeInPixels) - 1;
		int xEnd = xStart + (int)MathF.Ceiling(rectangle.Width * WorldUtils.PixelSizeInUnits) + 1;
		int yEnd = yStart + (int)MathF.Ceiling(rectangle.Height * WorldUtils.PixelSizeInUnits) + 1;

		for (int yy = yStart; yy < yEnd; yy++) {
			for (int xx = xStart; xx < xEnd; xx++) {
				var tile = Main.tile[xx, yy];

				if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) {
					WorldGen.KillTile_MakeTileDust(xx, yy, tile);
				}
			}
		}
	}

	public static void CreateOrUpdateDebrisAtPosition(Vector2 position, in Style style)
	{
		float time = TimeSystem.LogicTime;
		float sqrRange = style.RangeInPixels * style.RangeInPixels;

		foreach (ref var debris in CollectionsMarshal.AsSpan(DebrisInstances)) {
			float sqrDistance = Vector2.DistanceSquared(position, debris.Position);

			if (sqrDistance < sqrRange && debris.Style.Sound == style.Sound) {
				debris.StartTime = time;
				debris.EndTime = time + style.LengthInSeconds;
				debris.Position = position; //debris.Position = Vector2.Lerp(debris.Position, position, 0.5f);
				debris.Style.RangeInPixels = Math.Max(debris.Style.RangeInPixels, style.RangeInPixels);
				debris.Style.LengthInSeconds = Math.Max(debris.Style.LengthInSeconds, style.LengthInSeconds);
				return;
			}
		}

		DebrisInstances.Add(new Instance {
			Style = style,
			Position = position,
			StartTime = time,
			EndTime = time + style.LengthInSeconds,
		});
	}
}
