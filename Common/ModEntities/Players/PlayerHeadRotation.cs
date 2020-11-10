using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerHeadRotation : OverhaulPlayer
	{
		private float headRotation;
		private float targetHeadRotation;

		public override void PreUpdate()
		{
			const float LookStrength = 0.55f;

			var mouseWorld = player.GetModPlayer<PlayerDirectioning>().mouseWorld;
			Vector2 offset = mouseWorld - player.Center;

			if(Math.Sign(offset.X) == player.direction) {
				targetHeadRotation = (offset * player.direction).ToRotation() * LookStrength;
			}

			headRotation = MathHelper.Lerp(headRotation, targetHeadRotation, 16f * Systems.Time.TimeSystem.LogicDeltaTime);
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			if(!Main.gameMenu) {
				player.headRotation = headRotation;
			}
		}
	}
}
