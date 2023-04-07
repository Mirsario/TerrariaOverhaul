using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.PlayerEffects;

public class PlayerHeadRotation : ModPlayer
{
	public static readonly ConfigEntry<bool> EnablePlayerHeadRotation = new(ConfigSide.ClientOnly, "PlayerVisuals", nameof(EnablePlayerHeadRotation), () => true);

	private static bool active;

	private float headRotation;
	private float targetHeadRotation;

	public override void Load()
	{
		// The effect is exclusive to vanilla call paths, so that it doesn't screw with mods' custom draw calls.
		On_Main.DrawPlayers_AfterProjectiles += static (orig, main) => {
			active = true;

			orig(main);

			active = false;
		};

		On_Main.DrawPlayers_BehindNPCs += static (orig, main) => {
			active = true;

			orig(main);

			active = false;
		};
	}

	public override void PreUpdate()
	{
		const float LookStrength = 0.55f;

		if (Player.sleeping.isSleeping) {
			targetHeadRotation = 0;
		} else {
			var mouseWorld = Player.GetModPlayer<PlayerDirectioning>().MouseWorld;
			Vector2 offset = mouseWorld - Player.Center;

			if (Math.Sign(offset.X) == Player.direction) {
				targetHeadRotation = (offset * Player.direction).ToRotation() * LookStrength;
			} else {
				targetHeadRotation = 0;
			}
		}

		headRotation = MathHelper.Lerp(headRotation, targetHeadRotation, 16f * TimeSystem.LogicDeltaTime);
	}

	public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
	{
		if (!Main.gameMenu && EnablePlayerHeadRotation && active) {
			drawInfo.drawPlayer.headRotation = headRotation;
		}
	}
}
