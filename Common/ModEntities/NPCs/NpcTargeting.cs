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
		public float ReactionFactor { get; set; } = 4f;

		public Vector2 InterpolatedTargetVelocity { get; private set; }

		public override bool InstancePerEntity => true;

		public override bool PreAI(NPC npc)
		{
			var target = npc.GetTargetData();
			var targetVelocity = !target.Invalid ? target.Velocity : default;

			InterpolatedTargetVelocity = Vector2.Lerp(InterpolatedTargetVelocity, targetVelocity, ReactionFactor * TimeSystem.LogicDeltaTime);
				
			return true;
		}

		/*public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if(!DebugSystem.EnableDebugRendering) {
				return;
			}

			var target = npc.GetTargetData();

			if(target.Invalid) {
				return;
			}

			var prediction = GetPredictionWithSpeed(npc, npc.velocity.Length());

			DebugSystem.DrawCircle(prediction, 3f, Main.DiscoColor);
		}*/

		public Vector2 GetPredictionWithSpeed(NPC npc, float pixelsPerTickSpeed)
		{
			var target = npc.GetTargetData();
			float distance = Vector2.Distance(target.Center, npc.Center);
			float pixelsPerSecondSpeed = pixelsPerTickSpeed * TimeSystem.LogicFramerate;
			float delay = pixelsPerSecondSpeed / distance;

			return GetPredictionWithDelay(npc, delay);
		}

		public Vector2 GetPredictionWithDelay(NPC npc, float delay)
		{
			var target = npc.GetTargetData();

			return target.Center + InterpolatedTargetVelocity * TimeSystem.LogicFramerate * delay;
		}
	}
}
