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
	public override bool InstancePerEntity => true;

	private float prefferedDistanceX;
	private float prefferedDistanceY;

	private float horizontalSpeed;
	private float verticalSpeed;
	private float speedMultiplier;
	private float maxDistanceFactor;
	private bool needsAcceleration;

	private bool needsToUpdatePosition;
	private Vector2 currentTargetPosition;

	public void UpdateTargetPosition() => needsToUpdatePosition = true;

	public override void SetDefaults(NPC npc)
	{
		//Universal
		horizontalSpeed = 1.0f;
		verticalSpeed = 1.0f;
		prefferedDistanceX = 320.0f;
		prefferedDistanceY = 320.0f;
		maxDistanceFactor = 1.5f;
		speedMultiplier = 1.0f;
		needsToUpdatePosition = true;
	}

	// TO-DO: Better way of handling which NPCs should do this
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation) =>
		lateInstantiation && entity.type == ModContent.NPCType<EoCRework>();

	public void SetPrefferedDistance(float prefferedDistanceX, float prefferedDistanceY, float maxDistanceFactor = 1.5f)
	{
		this.prefferedDistanceX = prefferedDistanceX;
		this.prefferedDistanceY = prefferedDistanceY;
		this.maxDistanceFactor = maxDistanceFactor;
	}

	public void SetMovementSpeed(float newHorizontal = 1.0f, float newVertical = 1.0f)
	{
		horizontalSpeed = newHorizontal;
		verticalSpeed = newVertical;
	}
	public override void AI(NPC npc)
	{
		if (!npc.HasValidTarget) {
			return;
		}

		var target = npc.GetTarget();

		if(target == null) {
			return;
		}

		if(needsToUpdatePosition) {
			needsToUpdatePosition = false;
			currentTargetPosition = target.Center;
		}

		needsAcceleration = false;

		var currentDistanceX = Math.Abs(currentTargetPosition.X - npc.Center.X);
		var currentDistanceY = Math.Abs(currentTargetPosition.Y - npc.Center.Y);

		npc.velocity *= 0.98f;

		if (prefferedDistanceX != 0) {

			var directionMultiplier = 0;

			if (currentDistanceX < prefferedDistanceX) {
				directionMultiplier = -1;
			}

			if (currentDistanceX > prefferedDistanceX * maxDistanceFactor) {
				directionMultiplier = 1;
			}

			npc.velocity.X = (currentTargetPosition - npc.Center).SafeNormalize(-Vector2.UnitY).X * horizontalSpeed * speedMultiplier * directionMultiplier;

			needsAcceleration = directionMultiplier != 0;
		}

		if (prefferedDistanceY != 0) {

			var directionMultiplier = 0;

			if (currentDistanceY < prefferedDistanceY) {
				directionMultiplier = -1;
			}

			if (currentDistanceY > prefferedDistanceY * maxDistanceFactor) {
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
}
