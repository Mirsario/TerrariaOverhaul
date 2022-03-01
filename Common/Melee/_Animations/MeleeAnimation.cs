using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee
{
	public abstract class MeleeAnimation : ItemComponent
	{
		public abstract float GetItemRotation(Player player, Item item);

		public override void UseItemFrame(Item item, Player player)
		{
			if (!Enabled) {
				return;
			}

			float animationRotation = GetItemRotation(player, item);
			float weaponRotation = MathUtils.Modulo(animationRotation, MathHelper.TwoPi);
			float pitch = MathUtils.RadiansToPitch(weaponRotation);
			var weaponDirection = weaponRotation.ToRotationVector2();

			if (Math.Sign(weaponDirection.X) != player.direction) {
				pitch = weaponDirection.Y < 0f ? 1f : 0f;
			}

			player.bodyFrame = PlayerFrames.Use3.ToRectangle();

			Vector2 locationOffset;

			if (pitch > 0.95f) {
				player.bodyFrame = PlayerFrames.Use1.ToRectangle();
				locationOffset = new Vector2(-8f, -9f);
			} else if (pitch > 0.7f) {
				player.bodyFrame = PlayerFrames.Use2.ToRectangle();
				locationOffset = new Vector2(4f, -8f);
			} else if (pitch > 0.3f) {
				player.bodyFrame = PlayerFrames.Use3.ToRectangle();
				locationOffset = new Vector2(4f, 2f);
			} else if (pitch > 0.05f) {
				player.bodyFrame = PlayerFrames.Use4.ToRectangle();
				locationOffset = new Vector2(4f, 7f);
			} else {
				player.bodyFrame = PlayerFrames.Walk5.ToRectangle();
				locationOffset = new Vector2(-8f, 2f);
			}

			player.itemRotation = weaponRotation + MathHelper.PiOver4;

			if (player.direction < 0) {
				player.itemRotation += MathHelper.PiOver2;
			}

			player.itemLocation = player.Center + new Vector2(locationOffset.X * player.direction, locationOffset.Y);

			if (!Main.dedServ && DebugSystem.EnableDebugRendering) {
				DebugSystem.DrawCircle(player.itemLocation, 3f, Color.White);
			}
		}
	}
}
