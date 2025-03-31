// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Interface;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Bosses;

public sealed class BossDeathEffects : GlobalNPC
{
	public struct SharedBossLines()
	{
		public string[] DefeatedLines = [];
	}

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
				MultiplayerSystem.SendPacket(new Packet(npc.type, position));
			} else {
				Effect(npc.type, position);
			}
		}
	}

	private static void Modifier(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters musicParameters)
	{
		musicParameters.Volume = MathF.Min(musicParameters.Volume, volumeGradient.GetValue(1f - intensity));
	}

	private static void Effect(int npcType, Vector2 position)
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
				LengthInSeconds = 1.20f,
				FadeInLength = 0.35f,
				FadeOutLength = 5.0f,
				Zoom = +0.75f,
				UniqueId = "BossDeath",
			});
		}

		if (PlayBossDeathTransitionCue) {
			SoundEngine.PlaySound(new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Cinematics/BossDefeated") {
				Volume = 1.0f,
				SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
			});
		}

		if (BossLines.TryGet(npcType, out var lines)) {
			const float EffectLength = 7.5f;
			var defeatedFadeIn = (0.25f, 0.30f);

			OverlayText.Create(new OverlayTextLine {
				AnimationLength = EffectLength,
				Position = (new(0.5f, 0.25f), new(0f, 0f)),
				Text = lines.Name,
				FontOverride = FontAssets.DeathText,
				PrimaryColor = Color.White,
				OutlineColor = Color.DarkRed,

				FadeInEffect = (0.00f, 0.20f),
				FadeOutEffect = defeatedFadeIn,
			});
			OverlayText.Create(new OverlayTextLine {
				AnimationLength = EffectLength,
				Position = (new(0.5f, 0.25f), new(0f, 0f)),
				Text = lines.Name,
				FontOverride = FontAssets.DeathText,
				PrimaryColor = ColorUtils.FromHexRgb(0x8338c0),
				OutlineColor = Color.Black,

				FadeInEffect = defeatedFadeIn,
				FadeOutEffect = (0.60f, 1.00f),
				ShakeEffect = (10f, Vector2.One * 3f),
			});
			OverlayText.Create(new OverlayTextLine {
				AnimationLength = EffectLength,
				Position = (new(0.5f, 0.25f), new(0f, 60f)),
				Text = lines.DefeatLines[Main.rand.Next(lines.DefeatLines.Length)],
				FontOverride = FontAssets.DeathText,
				PrimaryColor = ColorUtils.FromHexRgb(0x8338c0),
				OutlineColor = Color.Black,

				FadeInEffect = defeatedFadeIn,
				FadeOutEffect = (0.60f, 1.00f),
				ShakeEffect = (10f, Vector2.One * 3f),
			});
		}
	}

	public sealed class Packet : NetPacket
	{
		public Packet(int npcType, Vector2 position)
		{
			Writer.Write7BitEncodedInt(npcType);
			Writer.WriteVector2(position);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			int npcType = reader.Read7BitEncodedInt();
			var position = reader.ReadVector2();
			if (npcType >= 0 && npcType < NPCLoader.NPCCount && !position.HasNaNs()) {
				Effect(npcType, position);
			}
		}
	}
}
