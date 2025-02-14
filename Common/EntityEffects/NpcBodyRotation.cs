// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.EntityEffects;

public sealed class NpcBodyRotation : GlobalNPC
{
	public static readonly ConfigEntry<bool> EnableEnemyTiltingEffects = new(ConfigSide.ClientOnly, true, "Visuals", "Enemies");

	private float usedRotationOffset;

	public float TiltingIntensity { get; set; } = 1.0f;

	public override bool InstancePerEntity => true;

	public override void SetDefaults(NPC npc)
	{
		if (npc.aiStyle == NPCAIStyleID.Slime) {
			TiltingIntensity *= 2f;
		}
	}

	public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (!EnableEnemyTiltingEffects || npc.IsABestiaryIconDummy) {
			usedRotationOffset = 0f;
			return true;
		}

		bool onGround = npc.collideY && npc.oldVelocity.Y > npc.velocity.Y;
		var movementDelta = npc.position - npc.oldPosition; // Not velocity.
		float movementOffset = BodyTilting.CalculateRotationOffset(movementDelta, onGround) * TiltingIntensity;
		float smoothing = onGround ? 0.001f : 0.002f;

		usedRotationOffset = MathUtils.Damp(usedRotationOffset, movementOffset, smoothing, TimeSystem.RenderDeltaTime);
		npc.rotation += usedRotationOffset;

		return true;
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		npc.rotation -= usedRotationOffset;
	}
}
