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
		public static PhysicalMaterial DefaultPhysicalMaterial { get; private set; }

		public override void Load()
		{
			DefaultPhysicalMaterial = ModContent.GetInstance<StonePhysicalMaterial>();
		}

		public override void Unload()
		{
			DefaultPhysicalMaterial = null;
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

			if(!PhysicalMaterialSystem.TryGetTilePhysicalMaterial(tile.type, out var material) || material is not IFootstepSoundProvider footstepProvider) {
				footstepProvider = (IFootstepSoundProvider)DefaultPhysicalMaterial;
			}

			SoundEngine.PlaySound(footstepProvider.FootstepSound, entity.Bottom);

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
