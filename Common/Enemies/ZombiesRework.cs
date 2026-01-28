// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Common.Footsteps;

namespace TerrariaOverhaul.Common.Enemies;

internal sealed class ZombiesRework : GlobalNPC
{
	public override void SetDefaults(NPC npc)
	{
		if (!EvilMonstersRework.EnableNormalEnemyEffects) return;
		if (!NPCID.Sets.Zombies[npc.type]) return;

		bool armed = false;

		if (!armed) {
			npc.GetGlobalNPC<NpcAudioEffects>().Data = new() {
				MeleeSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/Bite", 2) {
					Volume = 0.50f,
					PitchVariance = 0.15f,
					MaxInstances = 3,
				},
			};
		}
	}
}
