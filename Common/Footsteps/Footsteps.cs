// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Steamworks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
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

internal struct MaterialFootsteps : IComponent
{
	public SoundStyle? StepSound;
	public SoundStyle? JumpSound;
	public SoundStyle? LandSound;
}

internal class FootstepSystem : ModSystem
{
	public static Prefab DefaultFootstepSoundProvider { get; private set; }

	public override void OnModLoad()
	{
		DefaultFootstepSoundProvider = Prefabs.GetPrefab("StoneMaterial");
	}

	public static bool Footstep(Entity entity, FootstepType type, float volume = 1f, Point16? forcedPoint = null)
	{
		if (Main.dedServ) {
			return false;
		}

		var vec = entity.BottomLeft / 16f;
		var point = new Vector2Int((int)Math.Floor(vec.X), (int)Math.Ceiling(vec.Y));
		Tile tile = default;

		if (forcedPoint.HasValue && forcedPoint.Value.IsInWorld() && Main.tile.TryGet(forcedPoint.Value, out var tempTile) && tempTile.HasTile) {
			tile = tempTile;
		} else {
			for (int x = 0; x < 2; x++) {
				if (Main.tile.TryGet(point.X + x, point.Y, out tempTile) && tempTile.HasTile) {
					tile = tempTile;

					break;
				}
			}
		}

		if (tile == null) {
			return false;
		}

		Prefab? mainProvider = null;
		Prefab? extraProvider = null;
		float mainVolume = volume;
		float extraVolume = volume;

		// Check for nearby gore
		var entityRect = entity.GetRectangle();
		var worldPoint = ((Point16)point).ToWorldCoordinates();
		var extraRect = entityRect;
		extraRect.Inflate(8, 8);

		for (int i = 0; i < Main.maxGore; i++) {
			if (Main.gore[i] is OverhaulGore { active: true } gore && extraRect.Intersects(gore.AABBRectangle)
			&& gore is IMaterialProvider provider && provider.MaterialPrefab is { IsValid: true } mat) {
				if (type is FootstepType.Jump or FootstepType.Land) {
					bool strong = extraProvider == null && type is FootstepType.Land;
					var direction = (gore.Center.DirectionFrom(worldPoint) with { Y = -1f }).SafeNormalize(-Vector2.UnitY);
					gore.ApplyForce(direction * (strong ? 2.5f : 0.75f));
					gore.Damage(damageScale: strong ? 0.25f : 0.05f);
				}

				if (extraProvider == null) {
					extraProvider = mat;
					mainVolume *= 0.5f;
				}
			}
		}

		// Try to get a footstep provider from the tile
		if (mainProvider == null && PhysicalMaterials.TryGetTileMaterial(tile.TileType, out var material)) {
			mainProvider = material;
		}

		//TODO: Implement leaves footsteps when those are added.

		void PlaySound(Prefab? provider, float volume)
		{
			if (provider == null) return;

			ref readonly var footstepInfo = ref provider.Value.Get<MaterialFootsteps>();
			var sound = type switch {
				FootstepType.Jump => footstepInfo.JumpSound ?? footstepInfo.StepSound,
				FootstepType.Land => footstepInfo.LandSound ?? footstepInfo.StepSound,
				_ => footstepInfo.StepSound,
			};

			if (sound.HasValue) {
				SoundEngine.PlaySound(sound.Value with { Volume = sound.Value.Volume * volume }, entity.Bottom);
			}
		}

		PlaySound(mainProvider ?? DefaultFootstepSoundProvider, mainVolume);
		PlaySound(extraProvider, extraVolume);

		return true;
	}
}
