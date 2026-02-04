// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Footsteps;

namespace TerrariaOverhaul.Common.Enemies;

internal sealed class SlimesRework : GlobalNPC
{
	public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
	{
		return (entity.aiStyle == NPCAIStyleID.Slime || entity.type
			is NPCID.KingSlime
			or NPCID.TownSlimeBlue
			or NPCID.TownSlimeCopper
			or NPCID.TownSlimeGreen
			or NPCID.TownSlimeOld
			or NPCID.TownSlimePurple
			or NPCID.TownSlimeRainbow
			or NPCID.TownSlimeRed
			or NPCID.TownSlimeYellow
		);
	}

	public override void SetDefaults(NPC npc)
	{
		npc.GetGlobalNPC<NpcFootsteps>().Data = new() {
			SoundsOverride = new() {
				Jump = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Enemies/SlimeJump", 3) {
					Volume = npc.boss ? 0.6f : 0.21f,
					PitchVariance = 0.25f,
					MaxInstances = 4,
					SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				},
				Land = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Enemies/SlimeLand", 3) {
					Volume = npc.boss ? 0.6f : 0.21f,
					PitchVariance = 0.25f,
					MaxInstances = 4,
					SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				},
			},
		};
	}

	public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
	{
		target.AddBuff(BuffID.Slimed, 60);
	}

	public override void OnKill(NPC npc)
	{
		var center = npc.Center;
		int dimension = Math.Max(npc.width, npc.height);
		float range = 16f + (dimension * 3.5f);
		foreach (Player player in Main.ActivePlayers) {
			if (player.WithinRange(center, range)) {
				player.AddBuff(BuffID.Slimed, dimension * 4);
			}
		}
	}
}
