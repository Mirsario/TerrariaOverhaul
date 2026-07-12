// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Footsteps;

internal sealed class NpcFootsteps : GlobalNPC
{
	public class FootstepData
	{
		public uint LastFootstepTick;
		public Rectangle PreviousFrame;
		public Rectangle[] FootstepFrames = [];
		public FootstepSounds? SoundsOverride;
		public Vector2 OldVelocity;
		public Vector2 OldPosDelta;
		public Vector2 OldPosition;
	}

	private const uint FootstepCooldown = 3;

	public static readonly ConfigEntry<bool> EnableNpcFootstepSounds = new(ConfigSide.ClientOnly, true, "Ambience", "Enemies");
	public static readonly ConfigEntry<bool> EnableNpcFootstepInteractions = new(ConfigSide.ClientOnly, true, "Enemies", "BloodAndGore");
	public static readonly ConfigEntry<bool> EnableNpcLandingScreenShake = new(ConfigSide.ClientOnly, true, "Enemies", "Camera");
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
		if (Main.dedServ || Data is not FootstepData data) return;
		if (!EnableNpcFootstepSounds && !EnableNpcFootstepInteractions && !EnableNpcLandingScreenShake) return;

		var tick = Main.GameUpdateCount;
		var onGround = npc.velocity.Y == 0f;
		var wasOnGround = Data.OldVelocity.Y == 0f;
		var hopped = Data.OldVelocity.Y >= 0f && npc.velocity.Y < 0f;
		var currentFrame = npc.frame;

		FootstepType? footstepType = null;
		if (onGround != wasOnGround || hopped) {
			if (!onGround) {
				footstepType = FootstepType.Jump;
			} else {
				footstepType = FootstepType.Land;
			}
		} else if (onGround && data.PreviousFrame != default && currentFrame != data.PreviousFrame && data.FootstepFrames.Contains(npc.frame)) {
			footstepType = FootstepType.Default;
		}

		var posDelta = Data.OldPosition != default ? (npc.position - data.OldPosition) : default;
		if (npc.boss) posDelta *= 2;

		if (footstepType != null && (tick - data.LastFootstepTick) > FootstepCooldown && FootstepSystem.Footstep(new() {
			Kind = footstepType.Value,
			Hitbox = npc.Hitbox,
			Velocity = (posDelta, data.OldPosDelta),
			SoundsOverride = data.SoundsOverride,
			Volume = EnableNpcFootstepSounds ? 0.5f : 0,
			AllowGoreInteraction = EnableNpcFootstepInteractions,
			AllowScreenShake = EnableNpcLandingScreenShake,
			AllowParticles = !npc.wet,
		})) {
			data.LastFootstepTick = tick;
		}

		data.PreviousFrame = npc.frame;
		data.OldVelocity = npc.velocity;
		data.OldPosition = npc.position;
		data.OldPosDelta = posDelta;
	}
}
