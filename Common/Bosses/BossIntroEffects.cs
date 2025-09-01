// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Api.Camera;
using TerrariaOverhaul.Common.Interface;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Bosses;

internal sealed class BossIntroEffects : ModSystem
{
	private static GameTimer cooldown;

	public static bool? TryStartEffect(NPC npc, NpcBossIntroEffects npcEffects)
	{
		if (!BossLines.TryGet(npc.type, out var bossLines))
			return null;

		if (cooldown.Active)
			return false;

		const float MinDistance = 1000f;
		const float MinDistanceSqr = MinDistance * MinDistance;

		if (npc.Center.DistanceSQ(Main.LocalPlayer.Center) > MinDistanceSqr)
			return false;

		const float EffectLength = 5.0f;
		cooldown.Set((uint)(EffectLength * TimeSystem.LogicFramerate));

		if (Main.dedServ)
			return true;

		OverlayText.Create(new OverlayTextLine {
			AnimationLength = EffectLength,
			Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, -30f)),
			Text = bossLines.Name,
			Scale = Vector2.One * 1.10f,
			FontOverride = FontAssets.DeathText,
			PrimaryColor = Color.White,
			OutlineColor = Color.DarkRed,
			FadeInEffect = (0.00f, 0.20f),
			FadeOutEffect = (0.85f, 1.00f),
			ShakeEffect = (15f, Vector2.One * 3f),
		});
		if (bossLines.SubTitles.Length > 0) {
			OverlayText.Create(new OverlayTextLine {
				AnimationLength = EffectLength,
				Position = (new Vector2(0.5f, 0.25f), new Vector2(0f, 20f)),
				Text = bossLines.SubTitles[Main.rand.Next(bossLines.SubTitles.Length)],
				Scale = Vector2.One * 0.75f,
				FontOverride = FontAssets.DeathText,
				PrimaryColor = Color.White,
				OutlineColor = Color.DarkRed,
				FadeInEffect = (0.30f, 0.50f),
				FadeOutEffect = (0.85f, 1.00f),
				ShakeEffect = (15f, Vector2.One * 3f),
			});
		}

		CameraCurios.Create(new() {
			Identifier = "BossIntro",
			Position = npc.Center,
			Weight = 0.85f,
			Range = new(Min: 512f, Max: 2000f, Exponent: 2f),
			LengthInSeconds = EffectLength * 0.5f,
			FadeInLength = 0.35f,
			FadeOutLength = EffectLength * 0.5f,
			Zoom = +0.50f,
			Callback = new NpcTracker(npc).Center,
		});

		SoundEngine.PlaySound(new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Cinematics/BossEncounter") {
			Volume = 1.0f,
			PitchVariance = 0.1f,
			SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
		});

		return true;
	}
}

internal sealed class NpcBossIntroEffects : GlobalNPC
{
	public bool IntroPending;

	public override bool InstancePerEntity => true;

	public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
	{
		return npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
	}

	public override void OnSpawn(NPC npc, IEntitySource source)
	{
		if (Main.netMode == NetmodeID.Server) {
			MultiplayerSystem.SendPacket(new Packet(npc.whoAmI));
		} else {
			EnqueueEffect(npc);
		}
	}

	public override void PostAI(NPC npc)
	{
		if (IntroPending && BossIntroEffects.TryStartEffect(npc, this) != false) {
			IntroPending = false;
		}
	}

	public override bool? DrawHealthBar(NPC npc, byte hbPosition, ref float scale, ref Vector2 position)
	{
		return base.DrawHealthBar(npc, hbPosition, ref scale, ref position);
	}

	private void EnqueueEffect(NPC npc)
	{
		IntroPending = true;
		_ = npc;
	}

	public sealed class Packet : NetPacket
	{
		public Packet(int npcId)
		{
			Writer.Write(npcId);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			int npcId = reader.ReadInt32();

			if (npcId >= 0 && npcId < Main.maxNPCs && Main.npc[npcId] is { active: true } npc && npc.TryGetGlobalNPC(out NpcBossIntroEffects effects)) {
				effects.EnqueueEffect(npc);
			}
		}
	}
}
