// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Footsteps;

internal sealed class NpcFootsteps : GlobalNPC
{
	public class FootstepData
	{
		public Rectangle[] FootstepFrames = [];
		public uint LastFootstepTick;
		public Rectangle PreviousFrame;
	}

	private const uint FootstepCooldown = 3;

	public static readonly ConfigEntry<bool> EnableNpcFootsteps = new(ConfigSide.ClientOnly, true, "Ambience", "Enemies");
	private static readonly Rectangle[] genericWithIdle = [new(0, 5 * 56, 40, 56), new(0, 12 * 56, 40, 56)];
	private static readonly Rectangle[] genericWithoutIdle = [new(0, 4 * 56, 40, 56), new(0, 11 * 56, 40, 56)];

	public FootstepData? Data { get; set; }

	public override bool InstancePerEntity => true;

	public override void SetDefaults(NPC npc)
	{
	}
	public override void PostAI(NPC npc)
	{
		Initialize(npc);
		UpdateFootsteps(npc);
	}

	private void Initialize(NPC npc)
	{
		if (Data != null) return;

		// using var _ = new Logging.QuietExceptionHandle();
		// try { npc.FindFrame(); } catch { }

		int frameCount = Main.npcFrameCount[npc.type];
		// Generic
		if (npc.frame.X == 0 && npc.frame.Width == 40 && npc.frame.Height == 56) {
			if (npc.townNPC || (!npc.friendly && frameCount == 15)) {
				Data = new() { FootstepFrames = genericWithIdle };
			} else if (!npc.friendly && frameCount == 14) {
				Data = new() { FootstepFrames = genericWithoutIdle };
			}
		}
		// Zombies
		else if (npc.aiStyle == NPCAIStyleID.Fighter && frameCount == 3) {
			Data = new() { FootstepFrames = [npc.frame] };
		}
	}

	private void UpdateFootsteps(NPC npc)
	{
		if (Main.dedServ || !EnableNpcFootsteps || Data is not FootstepData data) {
			return;
		}

		var tick = Main.GameUpdateCount;
		var onGround = npc.velocity.Y == 0f;
		var wasOnGround = npc.oldVelocity.Y != 0f;
		var currentFrame = npc.frame;

		FootstepType? footstepType = null;
		if (onGround != wasOnGround && false) {
			if (!onGround) {
				footstepType = FootstepType.Jump;
			} else {
				footstepType = FootstepType.Land;
			}
		} else if (onGround && data.PreviousFrame != default && currentFrame != data.PreviousFrame && data.FootstepFrames.Contains(npc.frame)) {
			footstepType = FootstepType.Default;
		}

		if (footstepType != null && (tick - data.LastFootstepTick) > FootstepCooldown && FootstepSystem.Footstep(npc, footstepType.Value, volume: 0.5f)) {
			data.LastFootstepTick = tick;
		}

		data.PreviousFrame = npc.frame;
	}
}
