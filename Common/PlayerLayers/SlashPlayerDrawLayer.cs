using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic;

namespace TerrariaOverhaul.Common.PlayerLayers
{
	public class SlashPlayerDrawLayer : PlayerDrawLayer
	{
		private static readonly SpriteFrame TextureFrame = new SpriteFrame(1, 3);
		private static Asset<Texture2D> texture;

		public override void Load()
		{
			texture = Mod.GetTexture("Common/PlayerLayers/Slash");
		}
		public override void Unload()
		{
			texture = null;
		}
		//Layer settings
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeldItem);
		public override bool GetDefaultVisiblity(PlayerDrawSet drawInfo) => true;

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			var player = drawInfo.drawPlayer;

			if(player.itemAnimation <= 0) {
				return;
			}

			var item = player.HeldItem;

			if(!item.TryGetGlobalItem<MeleeWeapon>(out var meleeWeapon, false)) {
				return;
			}

			var frame = TextureFrame;

			frame.CurrentRow = (byte)((player.itemAnimation / (float)player.itemAnimationMax) * frame.RowCount);

			var attackDirection = meleeWeapon.AttackDirection;
			float attackAngle = meleeWeapon.AttackAngle;
			float attackRange = meleeWeapon.GetAttackRange(item);

			var tex = texture.Value;
			var position = player.Center + attackDirection * (attackRange / 2.2f);
			var sourceRectangle = frame.GetSourceRectangle(tex);
			float rotation = attackAngle;
			Vector2 origin = sourceRectangle.Size() * 0.5f;
			float scale = attackRange / 50f;
			var effect = player.direction > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

			drawInfo.DrawDataCache.Add(new DrawData(tex, position - Main.screenPosition, sourceRectangle, Color.White, rotation, origin, scale, effect, 0));

			/*var player = drawInfo.drawPlayer;

			var tex = texture.Value;
			Vector2 vectorA = new Vector2(tex.Width / 2, tex.Height / 2);
			Vector2 vectorB = Main.DrawPlayerItemPos(drawInfo.drawPlayer.gravDir, player.HeldItem.type);
			vectorA.Y = vectorB.Y;

			Vector2 position = drawInfo.ItemLocation + vectorA - Main.screenPosition;
			Vector2 origin = new Vector2(0f, tex.Height / 2);

			if(drawInfo.itemEffect != SpriteEffects.FlipHorizontally) {
				origin.X = -tex.Width * 0.4f;
			}

			drawInfo.DrawDataCache.Add(new DrawData(
				tex,
				position,
				null,
				Color.White,
				player.itemRotation,
				origin,
				1f,
				drawInfo.itemEffect,
				0
			));*/
		}
	}
}
