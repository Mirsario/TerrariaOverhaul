// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Bosses;

public sealed class BossDeathEffects : GlobalNPC
{
	public static readonly ConfigEntry<bool> EnableBossDeathMusicStop = new(ConfigSide.ClientOnly, true, "Music", "Bosses");
	public static readonly ConfigEntry<bool> PlayBossDeathTransitionCue = new(ConfigSide.ClientOnly, true, "Music", "Bosses");
	public static readonly ConfigEntry<bool> FocusCameraOnBossEvents = new(ConfigSide.ClientOnly, true, "Camera", "Bosses");

	private static readonly Gradient<float> volumeGradient = new(
		(0.00f, 0f),
		(0.75f, 0f),
		(1.00f, 1f)
	);

	public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
	{
		return npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
	}

	public override void OnKill(NPC npc)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || !npc.IsNPCValidForBestiaryKillCredit()) {
			return;
		}

		var countedBosses = ActiveEntities.NPCs.Where(n => n != npc && AppliesToEntity(n, false) && (npc.realLife < 0 || n.realLife != npc.realLife)).ToArray();

		if (countedBosses.Length <= 0) {
			var position = npc.Center;

			if (Main.netMode == NetmodeID.Server) {
				MultiplayerSystem.SendPacket(new Packet(position));
			} else {
				Effect(position);
			}
		}
	}

	private static void Modifier(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters musicParameters)
	{
		musicParameters.Volume = MathF.Min(musicParameters.Volume, volumeGradient.GetValue(1f - intensity));
	}

	private static void Effect(Vector2 position)
	{
		if (Main.dedServ) {
			return;
		}

		const float MuteTimeInSeconds = 7.5f;
		const float MaxDistance = 10000f;
		const float MaxDistanceSqr = MaxDistance * MaxDistance;

		if (Main.LocalPlayer.DistanceSQ(position) > MaxDistanceSqr) {
			return;
		}

		if (EnableBossDeathMusicStop) {
			int muteTimeInTicks = (int)(MuteTimeInSeconds * TimeSystem.LogicFramerate);
			AudioEffectsSystem.AddAudioEffectModifier(muteTimeInTicks, nameof(BossDeathEffects), Modifier);
		}

		if (FocusCameraOnBossEvents) {
			CameraCurios.Create(position, new() {
				Weight = 3.00f,
				Range = new(Min: 512f, Max: 1536f, Exponent: 2f),
				LengthInSeconds = 1.00f,
				FadeInLength = 0.35f,
				FadeOutLength = 3.0f,
				Zoom = 2f,
				UniqueId = "BossDeath",
			});
		}

		if (PlayBossDeathTransitionCue) {
			SoundEngine.PlaySound(new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Cinematics/Transition", 2) {
				Volume = 1.0f,
				PitchVariance = 0.1f,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
			});
		}
	}

	public sealed class Packet : NetPacket
	{
		public Packet(Vector2 position)
		{
			Writer.WriteVector2(position);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			var position = reader.ReadVector2();
			if (!position.HasNaNs()) {
				Effect(position);
			}
		}
	}
}
