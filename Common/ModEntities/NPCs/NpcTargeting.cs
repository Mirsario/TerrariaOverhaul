using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.Systems.Debugging;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public sealed class NpcTargeting : GlobalNPC
	{
		private Vector2[] interpolatedTargetVelocities = new Vector2[10];
		private Vector2 previousInterpolatedTargetVelocity;
		private Vector2 lastPredictedDebugPosition;

		public float ReactionFactor { get; set; } = 0.1f;
		public float? DebugProjectileSpeed { get; set; }

		public int ReactionDelayInTicks {
			get => interpolatedTargetVelocities.Length - 1;
			set {
				int newArraySize = Math.Max(value + 1, 1);

				if(newArraySize != interpolatedTargetVelocities.Length) {
					Array.Resize(ref interpolatedTargetVelocities, newArraySize);
				}
			}
		}

		public override bool InstancePerEntity => true;

		public override bool PreAI(NPC npc)
		{
			var target = npc.GetTargetData();
			var targetVelocity = !target.Invalid ? target.Velocity : default;

			// Update velocity history

			previousInterpolatedTargetVelocity = interpolatedTargetVelocities[0];

			Array.Copy(interpolatedTargetVelocities, 0, interpolatedTargetVelocities, 1, interpolatedTargetVelocities.Length - 1);

			interpolatedTargetVelocities[0] = Vector2.Lerp(previousInterpolatedTargetVelocity, targetVelocity, ReactionFactor);

			return true;
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if(!DebugSystem.EnableDebugRendering) {
				return;
			}

			var target = npc.GetTargetData();

			if(target.Invalid) {
				return;
			}

			const int PredictionRate = 20;

			if(Main.GameUpdateCount % PredictionRate == 0) {
				lastPredictedDebugPosition = GetPredictionWithDelay(npc, PredictionRate * TimeSystem.LogicDeltaTime);
			}

			DebugSystem.DrawCircle(lastPredictedDebugPosition, 7f, Color.LightGoldenrodYellow);

			if(DebugProjectileSpeed.HasValue) {
				var projectilePrediction = GetPredictionWithSpeed(npc, DebugProjectileSpeed.Value);

				DebugSystem.DrawCircle(projectilePrediction, 7f, Color.PaleVioletRed);
			}
		}

		public Vector2 GetPredictionWithSpeed(NPC npc, float pixelsPerTickSpeed)
		{
			var target = npc.GetTargetData();
			float distance = Vector2.Distance(target.Center, npc.Center);
			float pixelsPerSecondSpeed = pixelsPerTickSpeed * TimeSystem.LogicFramerate;
			float delay = distance / pixelsPerSecondSpeed;

			return GetPredictionWithDelay(npc, delay);
		}

		public Vector2 GetPredictionWithDelay(NPC npc, float delay)
		{
			var target = npc.GetTargetData();
			var predictedVelocity = interpolatedTargetVelocities[^1];
			//var targetAcceleration = targetVelocity.Y != 0f ? new Vector2(0f, Player.defaultGravity) : default;
			var targetAcceleration = interpolatedTargetVelocities[^2] - predictedVelocity;

			//predictedVelocity += targetAcceleration * TimeSystem.LogicFramerate * delay;

			return target.Center + (predictedVelocity * TimeSystem.LogicFramerate * delay);
		}
	}
}
