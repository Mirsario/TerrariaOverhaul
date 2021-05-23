using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic;
using TerrariaOverhaul.Common.ModEntities.Items.Utilities;
using TerrariaOverhaul.Common.Systems.TextureColors;
using TerrariaOverhaul.Utilities.DataStructures;

namespace TerrariaOverhaul.Common.PlayerLayers
{
	[Autoload(Side = ModSide.Client)]
	public class SlashPlayerDrawLayer : PlayerDrawLayer
	{
		private static readonly SpriteFrame TextureFrame = new(1, 3);
		private static Asset<Texture2D> texture;

		public override void Load()
		{
			texture = Mod.GetTexture($"{Utilities.ModPathUtils.GetDirectory(GetType())}/Slash");
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

			if(player.itemAnimation <= 0 || player.itemAnimationMax <= 0) {
				return;
			}

			float useProgress = player.itemAnimation / (float)player.itemAnimationMax;

			useProgress = (useProgress * 2f) - 1f;

			if(useProgress < 0) {
				return;
			}

			var item = player.HeldItem;

			if(!item.TryGetGlobalItem<MeleeWeapon>(out var meleeWeapon, false)) {
				return;
			}

			if(item.TryGetGlobalItem<ItemCharging>(out var itemCharging) && itemCharging.IsCharging) {
				return;
			}

			//Framing
			var frame = TextureFrame;

			frame.CurrentRow = (byte)(useProgress * frame.RowCount);

			//Attack info
			var attackDirection = meleeWeapon.AttackDirection;
			float attackAngle = meleeWeapon.AttackAngle;
			float attackRange = meleeWeapon.GetAttackRange(item);

			//Drawing info
			var tex = texture.Value;
			var position = player.Center + attackDirection * 2f;
			var sourceRectangle = frame.GetSourceRectangle(tex);
			float rotation = attackAngle;
			Vector2 origin = sourceRectangle.Size() * 0.5f;
			float scale = attackRange / 30f;
			var effect = ((player.direction > 0) ^ meleeWeapon.FlippedAttack) ? SpriteEffects.FlipVertically : SpriteEffects.None;

			//Color calculation
			Main.instance.LoadItem(item.type);

			const float MaxAlpha = 0.33f;

			var alphaGradient = new Gradient<float>(
				(0.00f, 0f),
				(0.25f, MaxAlpha),
				(0.75f, MaxAlpha),
				(1.00f, 0f)
			);

			var itemTextureAsset = TextureAssets.Item[item.type];
			var color = Lighting.GetColor(position.ToTileCoordinates()).MultiplyRGB(TextureColorSystem.GetAverageColor(itemTextureAsset)) * alphaGradient.GetValue(useProgress);

			//Drawing
			drawInfo.DrawDataCache.Add(new DrawData(tex, position - Main.screenPosition, sourceRectangle, color, rotation, origin, scale, effect, 0));
		}
	}
}
