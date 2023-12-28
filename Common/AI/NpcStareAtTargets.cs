using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.Bosses;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AI;

/// <summary>
/// Used to determine average position of all applicable players and get an angle towards that position.
/// </summary>
public class NpcStareAtTargets : GlobalNPC
{
	/// <summary> How fast it turns from current angle to target angle. </summary>
	private float turnSpeed;

	/// <summary> Resulting angle. </summary>
	public float StareAngle { get; private set; }

	/// <summary> Averaged-out position of all acquired targets. </summary>
	public Vector2 AveragePosition { get; private set; }

	public override bool InstancePerEntity => true;

	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		=> lateInstantiation && entity.type == ModContent.NPCType<EyeOfCthulhu>();

	public override void SetDefaults(NPC npc)
	{
		turnSpeed = 0.075f;
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

		// to-do: determine how far targets can be from the boss to be considered participants of the fight;
		// alternatively: determine which players are within the boss arena;
		List<Vector2> targets = Main.player.Where(x => x.active && x.whoAmI != 255 
		&& !x.dead 
		/*&& npc.Center.DistanceSQ(x.Center) <= 32000*/
		).Select(x => x.Center).ToList();

		// get average position;
		AveragePosition = target.Center; // assume that we only deal with one target initially;

		// sums up all positions of targets, divides by count to get average;
		// needs multiplayer testing as of now;
		AveragePosition = targets.Aggregate(Vector2.Zero, (s, v) => s + v) / (float)targets.Count;

		StareAngle = Utils.AngleLerp(StareAngle, (AveragePosition - npc.Center).ToRotation(), turnSpeed);
	}
}
