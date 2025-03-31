// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Footsteps;

public enum FootstepType
{
	Default,
	Jump,
	Land,
}

public struct MaterialFootsteps : IComponent
{
	public SoundStyle? StepSound;
	public SoundStyle? JumpSound;
	public SoundStyle? LandSound;
}

public class FootstepSystem : ModSystem
{
	public static Prefab DefaultFootstepSoundProvider { get; private set; }

	public override void OnModLoad()
	{
		DefaultFootstepSoundProvider = Prefabs.GetPrefab("StoneMaterial");
	}

	public static bool Footstep(Entity entity, FootstepType type, Point16? forcedPoint = null)
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

		Prefab? soundProvider = null;

		// Check for nearby gore
		var entityRect = entity.GetRectangle();

		for (int i = 0; i < Main.maxGore; i++) {
			if (Main.gore[i] is { active: true } gore && entityRect.Intersects(gore.AABBRectangle)
			&& gore is IMaterialProvider provider && provider.MaterialPrefab is { IsValid: true } mat) {
				soundProvider = mat;
				break;
			}
		}

		// Try to get a footstep provider from the tile
		if (soundProvider == null && PhysicalMaterials.TryGetTileMaterial(tile.TileType, out var material)) {
			soundProvider = material;
		}

		//TODO: Implement leaves footsteps when those are added.

		// Use default footstep provider in case of failure
		soundProvider ??= DefaultFootstepSoundProvider;

		ref readonly var footstepInfo = ref soundProvider.Value.Get<MaterialFootsteps>();
		var sound = type switch {
			FootstepType.Jump => footstepInfo.JumpSound ?? footstepInfo.StepSound,
			FootstepType.Land => footstepInfo.LandSound ?? footstepInfo.StepSound,
			_ => footstepInfo.StepSound,
		};

		if (sound.HasValue) {
			SoundEngine.PlaySound(sound.Value, entity.Bottom);
		}

		return true;
	}
}
