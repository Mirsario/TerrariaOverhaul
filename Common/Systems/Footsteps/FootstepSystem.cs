using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.PhysicalMaterials;
using TerrariaOverhaul.Core.Systems.PhysicalMaterials;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Systems.Footsteps
{
	public class FootstepSystem : ModSystem
	{
		public static IFootstepSoundProvider DefaultFootstepSoundProvider { get; private set; }

		public override void Load()
		{
			DefaultFootstepSoundProvider = ModContent.GetInstance<StonePhysicalMaterial>();
		}

		public override void Unload()
		{
			DefaultFootstepSoundProvider = null;
		}

		public static bool Footstep(Entity entity, FootstepType type, Point16? forcedPoint = null)
		{
			if(Main.dedServ) {
				return false;
			}

			var vec = entity.BottomLeft / 16f;
			var point = new Vector2Int((int)Math.Floor(vec.X), (int)Math.Ceiling(vec.Y));
			Tile tile = null;

			if(forcedPoint.HasValue && forcedPoint.Value.IsInWorld() && Main.tile.TryGet(forcedPoint.Value, out var tempTile) && tempTile.IsActive) {
				tile = tempTile;
			} else {
				for(int x = 0; x < 2; x++) {
					if(Main.tile.TryGet(point.X + x, point.Y, out tempTile) && tempTile.IsActive) {
						tile = tempTile;

						break;
					}
				}
			}

			if(tile == null) {
				return false;
			}

			IFootstepSoundProvider soundProvider = null;

			// Check for nearby gore
			var entityRect = entity.GetRectangle();

			for(int i = 0; i < Main.maxGore; i++) {
				var gore = Main.gore[i];

				if(gore == null || !gore.active || !entityRect.Intersects(gore.AABBRectangle)) {
					continue;
				}

				if(gore is not IPhysicalMaterialProvider materialProvider) {
					continue;
				}

				if(materialProvider.PhysicalMaterial is IFootstepSoundProvider goreFootstepProvider) {
					soundProvider ??= goreFootstepProvider;
					break;
				}
			}

			// Try to get a footstep provider from the tile
			if(soundProvider == null && PhysicalMaterialSystem.TryGetTilePhysicalMaterial(tile.type, out var material)) {
				soundProvider = material as IFootstepSoundProvider;
			}

			//TODO: Implement leaves footsteps when those are added.

			// Use default footstep provider in case of failure
			soundProvider ??= DefaultFootstepSoundProvider;

			var sound = type switch {
				FootstepType.Jump => soundProvider.JumpFootstepSound,
				FootstepType.Land => soundProvider.LandFootstepSound,
				_ => soundProvider.FootstepSound
			};

			SoundEngine.PlaySound(sound, entity.Bottom);

			return true;
		}
	}
}
