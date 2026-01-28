// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.EntityEffects;

internal sealed class NpcAudioEffects : GlobalNPC
{
	public sealed class MeleePacket : NetPacket
	{
		public MeleePacket(NPC npc)
		{
			Writer.Write((ushort)npc.whoAmI);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			ushort npcIndex = reader.ReadUInt16();
			if (npcIndex >= Main.maxNPCs || Main.npc[npcIndex] is not { active: true } npc) return;

			if (npc.TryGetGlobalNPC(out NpcAudioEffects effects)) effects.MeleeEffect(npc);

			if (Main.netMode == NetmodeID.Server) {
				MultiplayerSystem.SendPacket(new MeleePacket(npc), ignoreClient: sender);
			}
		}
	}

	public class EffectData
	{
		// Approach Sound
		public SoundStyle? ApproachSound;
		public ScreenShake ApproachScreenShake;
		public float ApproachVelocity = 3.25f;
		public float ApproachDistance = 768f;
		public (int Min, int Max) ApproachSoundCooldown;
		// Random Sound
		public SoundStyle? RandomSound;
		public (int Min, int Max) RandomSoundCooldown;
		// Movement Sound
		public SoundStyle? MovementSound;
		public (float MinSpeed, float MaxSpeed, float MinPitch, float MaxPitch) MovementSoundVelocityPitching = (2.5f, 10f, -0.50f, 0.50f);
		// Damage Sound
		public SoundStyle? MeleeSound;

		internal ulong randomSoundCooldownEndTime;
		internal ulong approachSoundCooldownEndTime;
		internal float lastNonUndergroundDistanceToClosestTarget;
		internal SlotId movementSoundId;
	}

	public EffectData? Data;
	public Vector2 OldPosition;

	public override bool InstancePerEntity => true;

	public override bool PreAI(NPC npc)
	{
		OldPosition = npc.position;

		return base.PreAI(npc);
	}
	public override void PostAI(NPC npc)
	{
		if (Main.dedServ || Data is not EffectData data) {
			return;
		}

		var center = npc.Center;
		var centerPoint = center.ToTileCoordinates();

		if (!Main.tile.TryGet(centerPoint, out var centerTile)) {
			return;
		}

		var perceivedVelocity = npc.position - npc.oldPosition;
		ulong gameUpdateCount = Main.GameUpdateCount;

		// Approach Effects
		if (data.ApproachSound != null && (!centerTile.HasTile || !Main.tileSolid[centerTile.TileType] || Main.tileSolidTop[centerTile.TileType])) {
			float approachDistanceSqr = data.ApproachDistance * data.ApproachDistance;
			float minTargetDistance = ActiveEntities.Players.Min(p => Vector2.DistanceSquared(p.Center, center));

			if (minTargetDistance <= approachDistanceSqr
				&& data.lastNonUndergroundDistanceToClosestTarget > approachDistanceSqr
				&& gameUpdateCount >= data.approachSoundCooldownEndTime
				&& perceivedVelocity.LengthSquared() >= data.ApproachVelocity * data.ApproachVelocity
			) {
				if (SoundEngine.PlaySound(in data.ApproachSound, center, new NpcTracker(npc).AudioCallback) != SlotId.Invalid) {
					ScreenShakeSystem.New(data.ApproachScreenShake, center);
					data.approachSoundCooldownEndTime = gameUpdateCount + (ulong)Main.rand.Next(data.ApproachSoundCooldown.Min, data.ApproachSoundCooldown.Max);
				}
			}

			data.lastNonUndergroundDistanceToClosestTarget = minTargetDistance;
		}

		// Random Effects
		if (data.RandomSound != null && gameUpdateCount >= data.randomSoundCooldownEndTime && data.RandomSoundCooldown.Max != 0) {
			if (data.randomSoundCooldownEndTime == 0 || SoundEngine.PlaySound(data.RandomSound, center, new NpcTracker(npc).AudioCallback) != SlotId.Invalid) {
				data.randomSoundCooldownEndTime = gameUpdateCount + (ulong)Main.rand.Next(data.RandomSoundCooldown.Min, data.RandomSoundCooldown.Max);
			}
		}

		if (data.MovementSound != null && !SoundEngine.TryGetActiveSound(data.movementSoundId, out _)) {
			object boxedIndex = npc.whoAmI;
			data.movementSoundId = SoundEngine.PlaySound(in data.MovementSound, center, s => MovementSoundCallback(s, boxedIndex));
		}
	}

	public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
	{
		MeleeEffect(npc);

		if (Main.netMode == NetmodeID.MultiplayerClient)
			MultiplayerSystem.SendPacket(new MeleePacket(npc));
	}
	public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
	{
		MeleeEffect(npc);

		if (Main.netMode == NetmodeID.Server)
			MultiplayerSystem.SendPacket(new MeleePacket(npc));
	}

	public void MeleeEffect(NPC npc)
	{
		if (Data?.MeleeSound is { } sound) {
			SoundEngine.PlaySound(sound, npc.Center);
		}
	}

	private static bool MovementSoundCallback(ActiveSound sound, object boxedIndex)
	{
		if (Main.npc[(int)boxedIndex] is not NPC { active: true } npc
		|| !npc.TryGetGlobalNPC(out NpcAudioEffects effects)
		|| effects.Data == null) {
			return false;
		}

		var cameraCenter = CameraSystem.ScreenCenter;
		var nearestSegment = npc;
		float nearestDistanceSqr = float.PositiveInfinity;

		foreach (var otherNpc in ActiveEntities.NPCs) {
			if (otherNpc.realLife != npc.realLife) {
				continue;
			}

			float distanceSqr = Vector2.DistanceSquared(otherNpc.position, cameraCenter);
			if (distanceSqr < nearestDistanceSqr) {
				nearestSegment = otherNpc;
				nearestDistanceSqr = distanceSqr;
			}
		}

		if (nearestSegment.TryGetGlobalNPC(out NpcAudioEffects segmentEffects)) {
			var (minSpeed, maxSpeed, minPitch, maxPitch) = effects.Data.MovementSoundVelocityPitching;
			float segmentSpeed = (nearestSegment.position - segmentEffects.OldPosition).Length();

			sound.Position = nearestSegment.Center;
			sound.Pitch = MathHelper.Lerp(minPitch, maxPitch, MathUtils.Clamp01((segmentSpeed - minSpeed) / (maxSpeed - minSpeed)));
		}

		return true;
	}
}
