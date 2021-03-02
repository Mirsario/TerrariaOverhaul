using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerRotation : PlayerBase
	{
		private const float GlobalRotationOffsetScale = 0.025f;

		public float rotation;
		public float rotationOffsetScale;

		public override void PreUpdate()
		{
			if(Player.dead) {
				return;
			}

			Player.fullRotationOrigin = new Vector2(11, 22);
		}
		public override void PostUpdate()
		{
			if(rotationOffsetScale != 0f) {
				float movementRotation;

				if(Player.OnGround()) {
					movementRotation = Player.velocity.X * (Player.velocity.X < Main.MouseWorld.X ? 1f : -1f) * GlobalRotationOffsetScale;
				} else {
					movementRotation = MathHelper.Clamp(Player.velocity.Y * Math.Sign(Player.velocity.X) * -0.015f, -0.4f, 0.4f);
				}

				rotation += movementRotation;

				//TODO: If swimming, multiply by 4.
			}

			/*while(rotation>=MathHelper.TwoPi) {
				rotation -= MathHelper.TwoPi;
			}

			while(rotation<=-MathHelper.TwoPi) {
				rotation += MathHelper.TwoPi;
			}*/

			if(!Player.mount.Active) {
				Player.fullRotation = rotation * Player.gravDir;
			}

			rotation = 0f;
			rotationOffsetScale = 1f;
		}
	}
}
