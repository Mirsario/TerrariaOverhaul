using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.Bosses;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AI;

// TO-DO: Smooth out movement;
public class NpcGetComfortableDistance : GlobalNPC
{
	// Configuration
	private Vector2 preferredDistance = Vector2.One * 320f;
	private float horizontalSpeed = 1f;
	private float verticalSpeed = 1f;
	private float speedMultiplier = 1f;
	private float maxDistanceFactor = 1.5f;
	// Etc.
	private bool needsAcceleration;
	private Vector2 currentTargetPosition;
	private bool needsToUpdatePosition = true;

	public override bool InstancePerEntity => true;

	//TODO: Better way of handling which NPCs should do this
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		=> lateInstantiation && entity.type == ModContent.NPCType<EyeOfCthulhu>();

	public override void SetDefaults(NPC npc)
	{
		needsToUpdatePosition = true;
	}

	public override void AI(NPC npc)
	{
		if (!npc.HasValidTarget) {
			return;
		}

		var target = npc.GetTarget();

		if (target == null) {
			return;
		}

		if (needsToUpdatePosition) {
			needsToUpdatePosition = false;
			currentTargetPosition = target.Center;
		}

		needsAcceleration = false;

		var currentDistance = Vector2Utils.Abs(currentTargetPosition - npc.Center);

		npc.velocity *= 0.98f;

		if (preferredDistance.X != 0f) {
			int directionMultiplier = 0;

			if (currentDistance.X < preferredDistance.X) {
				directionMultiplier = -1;
			}

			if (currentDistance.X > preferredDistance.X * maxDistanceFactor) {
				directionMultiplier = 1;
			}

			npc.velocity.X = (currentTargetPosition - npc.Center).SafeNormalize(-Vector2.UnitY).X * horizontalSpeed * speedMultiplier * directionMultiplier;

			needsAcceleration = directionMultiplier != 0;
		}

		if (preferredDistance.Y != 0f) {
			int directionMultiplier = 0;

			if (currentDistance.Y < preferredDistance.Y) {
				directionMultiplier = -1;
			}

			if (currentDistance.Y > preferredDistance.Y * maxDistanceFactor) {
				directionMultiplier = 1;
			}

			npc.velocity.Y = (currentTargetPosition - npc.Center).SafeNormalize(-Vector2.UnitY).Y * verticalSpeed * speedMultiplier * directionMultiplier;

			if (!needsAcceleration) {
				needsAcceleration = directionMultiplier != 0;
			}
		}

		if (needsAcceleration) {
			speedMultiplier += 0.05f;
		} else {
			speedMultiplier -= 0.2f;
		}

		speedMultiplier = Math.Clamp(speedMultiplier, 0.05f, 5f);
	}

	public void UpdateTargetPosition()
	{
		needsToUpdatePosition = true;
	}

	public void SetPreferredDistance(Vector2 preferredDistance, float maxDistanceFactor = 1.5f)
	{
		this.preferredDistance = preferredDistance;
		this.maxDistanceFactor = maxDistanceFactor;
	}

	public void SetMovementSpeed(float newHorizontal = 1.0f, float newVertical = 1.0f)
	{
		horizontalSpeed = newHorizontal;
		verticalSpeed = newVertical;
	}
}
