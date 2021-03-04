using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.Tags;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Systems.Footsteps
{
	public class FootstepSystem : ModSystem
	{
		private static SoundStyle defaultFoostepSound;
		private static Dictionary<TagData, SoundStyle> footstepSoundsByTag;

		public override void Load()
		{
			const float Volume = 0.5f;

			defaultFoostepSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Stone/Step", 11, volume: Volume);
			footstepSoundsByTag = new Dictionary<TagData, SoundStyle> {
				{ OverhaulTileTags.Stone, defaultFoostepSound },
				{ OverhaulTileTags.Dirt, new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Dirt/Step", 11, volume: Volume) },
				{ OverhaulTileTags.Grass, new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Grass/Step", 10, volume: Volume) },
				{ OverhaulTileTags.Sand, new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Sand/Step", 11, volume: Volume) },
				{ OverhaulTileTags.Snow, new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Snow/Step", 11, volume: Volume) },
				{ OverhaulTileTags.Wood, new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Wood/Step", 11, volume: Volume) },
				{ OverhaulTileTags.Mud, new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Footsteps/Gross/Step", 3, volume: Volume) } //TODO: Update with better audio.
				//TODO: Add leaves
			};
		}
		public override void Unload()
		{
			defaultFoostepSound = null;
			footstepSoundsByTag = null;
		}

		public static SoundStyle GetTileFootstepSound(Tile tile)
		{
			foreach(var pair in footstepSoundsByTag) {
				if(pair.Key.Has(tile.type)) {
					return pair.Value;
				}
			}

			return defaultFoostepSound;
		}
		public static bool Foostep(Entity entity, Point16? forcedPoint = null)
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

			SoundEngine.PlaySound(GetTileFootstepSound(tile), entity.Bottom);

			return true;

			/*if(data!=0) {
				int o = data.Overlay;

				if(o==TileOverlayID.Snow) {
					footstepInstancer = OverhaulTiles.footstepSounds[(byte)FootstepType.Snow];
				} else if(o==TileOverlayID.LeavesGreen || o==TileOverlayID.LeavesRed || o==TileOverlayID.LeavesYellow) {
					footstepInstancer = OverhaulTiles.footstepSounds[(byte)FootstepType.Grass];
				}
			}

			int goreTouched = 0;

			for(int i = 0;i<Main.maxGore;i++) {
				var gore = Main.gore[i];

				if(gore!=null && gore.active && entityRect.Intersects(gore.GetRect())) {
					var gorePreset = GoreSystem.GetGorePreset(gore.type);

					if(gorePreset.bleeds && gorePreset.playHitSounds) {
						goreTouched++;

						if(goreTouched>=2) {
							break;
						}
					}
				}
			}

			if(goreTouched>=2) {
				footstepInstancer = OverhaulTiles.footstepSounds[(byte)FootstepType.Gross];
			}*/

			//Footsteps
			/*var instance = footstepInstancer(entity.Bottom);

			if(instance!=null) {
				instance.volume *= volume;
				instance.maxDistance *= 0.33f;
			}*/
		}
	}
}
