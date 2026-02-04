// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Steamworks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Footsteps;

public enum FootstepType
{
	Default,
	Jump,
	Land,
}

internal struct FootstepSounds : IComponent
{
	public SoundStyle? Step;
	public SoundStyle? Jump;
	public SoundStyle? Land;
}

internal struct FootstepCtx()
{
	public bool GoreInteraction = true;
	public float Volume = 1f;
	public required FootstepType Kind;
	public required Rectangle Hitbox;
	public required (Vector2 Cur, Vector2 Old) Velocity;
	public Point16? PointOverride;
	public FootstepSounds? SoundsOverride;
}

internal class FootstepSystem : ModSystem
{
	public static Prefab DefaultFootstepSoundProvider { get; private set; }

	public override void OnModLoad()
	{
		DefaultFootstepSoundProvider = Prefabs.Get("StoneMaterial");
	}

	public static bool Footstep(in FootstepCtx ctx)
	{
		if (Main.dedServ) return false;

		var kind = ctx.Kind;
		var tilePos = ctx.Hitbox.Bottom().ToTileCoordinates16();
		Tile? tile = null;

		// Find a valid tile.
		if (ctx.PointOverride is { } p && p.IsInWorld() && Main.tile.TryGet(p, out Tile t) && t.HasTile && Main.tileSolid[t.TileType]) {
			tile = t;
		} else for (int xMax = (int)MathF.Ceiling(ctx.Hitbox.Width / 16f), i = 0; i < xMax; i++) {
			int xOffset = (i / 2) * (i % 2 == 0 ? 1 : -1);
			if (Main.tile.TryGet(tilePos.X + xOffset, tilePos.Y, out t) && t.HasTile) {
				tile = t;
				break;
			}
		}

		var worldPos = tilePos.ToWorldCoordinates();
		Prefab? mainProvider = null;
		Prefab? extraProvider = null;
		float mainVolume = ctx.Volume;
		float extraVolume = ctx.Volume;

		// Try to get a footstep provider from the tile
		if (mainProvider == null && tile != null && PhysicalMaterials.TryGetTileMaterial(tile.Value.TileType, out var material)) {
			mainProvider = material;
		}

		// Check for nearby gore
		var collisionRect = ctx.Hitbox.Inflated(8, 8);
		for (int i = 0; i < Main.maxGore; i++) {
			if (Main.gore[i] is not OverhaulGore { active: true } gore) continue;
			if (gore.MaterialPrefab is not { IsValid: true } mat) continue;
			if (!collisionRect.Intersects(gore.AABBRectangle)) continue;

			if (kind is FootstepType.Jump or FootstepType.Land) {
				bool strong = extraProvider == null && kind is FootstepType.Land;
				var direction = (gore.Center.DirectionFrom(worldPos) with { Y = -1f }).SafeNormalize(-Vector2.UnitY);
				gore.ApplyForce(direction * (strong ? 2.5f : 0.75f));
				gore.Damage(damageScale: strong ? 0.25f : 0.05f);
			}

			if (extraProvider == null) {
				extraProvider = mat;
				mainVolume *= 0.5f;
			}
		}

		//TODO: Implement leaves footsteps when those are added.

		void PlaySound(float volume, FootstepSounds? footstepSounds)
		{
			if (footstepSounds is not { } sounds) return;

			var sound = kind switch {
				FootstepType.Jump => sounds.Jump ?? sounds.Step,
				FootstepType.Land => sounds.Land ?? sounds.Step,
				_ => sounds.Step,
			};

			if (sound.HasValue) {
				SoundEngine.PlaySound(sound.Value with { Volume = sound.Value.Volume * volume }, worldPos);
			}
		}

		PlaySound(mainVolume, ctx.SoundsOverride ?? ((mainProvider ?? DefaultFootstepSoundProvider).Get<FootstepSounds>()));
		PlaySound(extraVolume, extraProvider?.Get<FootstepSounds>());

		return true;
	}
}
