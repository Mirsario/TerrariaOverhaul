using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerRotation : OverhaulPlayer
	{
		private const float GlobalRotationOffsetScale = 0.025f;

		public float rotation;
		public float rotationOffsetScale;

		public override void PreUpdate()
		{
			if(player.dead) {
				return;
			}

			player.fullRotationOrigin = new Vector2(11,22);
		}
		public override void PostUpdate()
		{
			if(rotationOffsetScale != 0f) {
				float movementRotation;

				if(player.OnGround()) {
					movementRotation = player.velocity.X*(player.velocity.X<Main.MouseWorld.X ? 1f : -1f)*GlobalRotationOffsetScale;
				} else {
					movementRotation = MathHelper.Clamp(player.velocity.Y*Math.Sign(player.velocity.X)*-0.015f,-0.4f,0.4f);
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

			player.fullRotation = rotation;

			rotation = 0f;
			rotationOffsetScale = 1f;
		}
	}
}
