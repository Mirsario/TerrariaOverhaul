using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Core.Systems.Debugging;

namespace TerrariaOverhaul.Utilities
{
	public static class CollisionUtils
	{
		public static bool CheckRectangleVsCircleCollision(Rectangle aabb, Vector2 circleCenter, float circleRadius)
			=> CheckRectangleVsCircleCollision(aabb, circleCenter, circleRadius, out _);

		public static bool CheckRectangleVsCircleCollision(Rectangle aabb, Vector2 circleCenter, float circleRadius, out Vector2 closestPoint)
		{
			if(!Main.dedServ && DebugSystem.EnableDebugRendering) {
				DebugSystem.DrawCircle(circleCenter, circleRadius, Color.White);
			}

			Vector2 aabbHalfSize = aabb.Size() * 0.5f;
			Vector2 aabbCenter = aabb.Center();

			Vector2 difference = circleCenter - aabbCenter;
			Vector2 clampedDifference = Vector2.Clamp(difference, -aabbHalfSize, aabbHalfSize);

			closestPoint = aabbCenter + clampedDifference;

			return (closestPoint - circleCenter).LengthSquared() < circleRadius * circleRadius;
		}

		public static bool CheckRectangleVsArcCollision(Rectangle aabb, Vector2 arcCenter, float arcAngle, float arcRadius, float arcDistance)
			=> CheckRectangleVsArcCollision(aabb, arcCenter, arcAngle, arcRadius, arcDistance, out _);

		public static bool CheckRectangleVsArcCollision(Rectangle aabb, Vector2 arcCenter, float arcAngle, float arcRadius, float arcDistance, out Vector2 closestPoint)
		{
			float halfRadius = arcRadius * 0.5f;

			if(!Main.dedServ && DebugSystem.EnableDebugRendering) {
				DebugSystem.DrawLine(arcCenter, arcCenter + new Vector2(arcDistance, 0f).RotatedBy(arcAngle - halfRadius), Color.White);
				DebugSystem.DrawLine(arcCenter, arcCenter + new Vector2(arcDistance, 0f).RotatedBy(arcAngle), Color.White);
				DebugSystem.DrawLine(arcCenter, arcCenter + new Vector2(arcDistance, 0f).RotatedBy(arcAngle + halfRadius), Color.White);
			}

			if(!CheckRectangleVsCircleCollision(aabb, arcCenter, arcDistance, out closestPoint)) {
				return false;
			}

			//TODO: Lame. Doesn't work right if the arc is smaller than the rectangle.
			bool CheckAngle(Vector2 point)
			{
				float angle = (point - arcCenter).ToRotation();
				float diff = (arcAngle - angle + MathHelper.Pi + MathHelper.TwoPi) % MathHelper.TwoPi - MathHelper.Pi;

				return diff <= halfRadius && diff >= -halfRadius;
			}

			return CheckAngle(aabb.TopLeft()) || CheckAngle(aabb.TopRight()) || CheckAngle(aabb.BottomLeft()) || CheckAngle(aabb.BottomRight());
		}
	}
}
