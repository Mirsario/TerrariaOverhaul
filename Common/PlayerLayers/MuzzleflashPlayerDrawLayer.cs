using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns;
using TerrariaOverhaul.Core.Systems.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.PlayerLayers
{
	[Autoload(Side = ModSide.Client)]
	public class MuzzleflashPlayerDrawLayer : PlayerDrawLayer
	{
		private static Asset<Texture2D> texture;
		private static Dictionary<int, Vector2> gunBarrelEndPositions;

		//Assets
		public override void Load()
		{
			texture = Mod.Assets.Request<Texture2D>($"{ModPathUtils.GetDirectory(GetType())}/Muzzleflash");
			gunBarrelEndPositions = new Dictionary<int, Vector2>();
		}
		
		public override void Unload()
		{
			texture = null;
			gunBarrelEndPositions = null;
		}
		//Layer settings
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);
		public override bool GetDefaultVisiblity(PlayerDrawSet drawInfo) => true;

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			var player = drawInfo.drawPlayer;

			var item = player.HeldItem;

			if(item?.IsAir != false || !item.TryGetGlobalItem<Gun>(out var gun, false) || gun.MuzzleflashTime <= 0) {
				return;
			}

			Main.instance.LoadItem(item.type);

			var itemTexture = TextureAssets.Item[item.type].Value;
			var gunBarrelEnd = GetGunBarrelEndPosition(item.type, itemTexture) * item.scale;

			for(int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				var data = drawInfo.DrawDataCache[i];

				if(data.texture == itemTexture) {
					var gunPosition = data.position;
					var gunFixedOrigin = player.direction > 0 ? data.origin : (Vector2.UnitX * data.texture.Width - data.origin);

					if(DebugSystem.EnableDebugRendering) {
						DebugSystem.DrawCircle(gunPosition + Main.screenPosition, 4f, Color.White);
					}

					var originOffset = new Vector2(
						(gunBarrelEnd.X - gunFixedOrigin.X) * player.direction,
						(-gunFixedOrigin.Y * player.direction + gunBarrelEnd.Y)
					);

					var muzzleflashTexture = texture.Value;
					var muzzleflashPosition = gunPosition + originOffset.RotatedBy(player.itemRotation);
					var muzzleflashOrigin = new Vector2(player.direction < 0 ? muzzleflashTexture.Width : 0f, muzzleflashTexture.Height * 0.5f);

					drawInfo.DrawDataCache.Insert(i++, new DrawData(muzzleflashTexture, muzzleflashPosition, null, Color.White, player.itemRotation, muzzleflashOrigin, 1f, drawInfo.itemEffect, 0));

					Lighting.AddLight(muzzleflashPosition, Color.Gold.ToVector3());

					break;
				}
			}
		}

		/// <summary> Tries to calculate the center of the end of a gun's barrel based on its texture. </summary>
		private static Vector2 GetGunBarrelEndPosition(int type, Texture2D texture)
		{
			if(gunBarrelEndPositions.TryGetValue(type, out var result)) {
				return result;
			}

			var surface = new Surface<Color>(texture.Width, texture.Height);

			texture.GetData(surface.Data);

			var columnPoints = new List<Vector2>();

			for(int x = surface.Width - 1; x >= 0; x--) {
				bool columnIsEmpty = true;

				for(int y = 0; y < surface.Height; y++) {
					if(surface[x, y].A > 0) {
						columnIsEmpty = false;

						columnPoints.Add(new Vector2(x, y));
					}
				}

				if(!columnIsEmpty) {
					break;
				}
			}

			result = default;

			if(columnPoints.Count > 0) {
				foreach(var value in columnPoints) {
					result += value;
				}

				result /= columnPoints.Count;
			}

			gunBarrelEndPositions[type] = result;

			return result;
		}
	}
}
