using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Time;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerHeadRotation : ModPlayer
	{
		public static readonly ConfigEntry<bool> EnablePlayerHeadRotation = new(ConfigSide.ClientOnly, "PlayerVisuals", nameof(EnablePlayerHeadRotation), () => true);

		private float headRotation;
		private float targetHeadRotation;

		public override void PreUpdate()
		{
			const float LookStrength = 0.55f;

			var mouseWorld = Player.GetModPlayer<PlayerDirectioning>().MouseWorld;
			Vector2 offset = mouseWorld - Player.Center;

			if (Math.Sign(offset.X) == Player.direction) {
				targetHeadRotation = (offset * Player.direction).ToRotation() * LookStrength;
			}

			headRotation = MathHelper.Lerp(headRotation, targetHeadRotation, 16f * TimeSystem.LogicDeltaTime);
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			if (!Main.gameMenu && EnablePlayerHeadRotation) {
				Player.headRotation = headRotation;
			}
		}
	}
}
