// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

// Unused content, may be reintroduced in the future.
#if false
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerrariaOverhaul.Content.NPCs.Monsters.TheAshes;

internal class AshZombie : AshMonster
{
	protected override int BaseNPC => NPCID.BloodZombie;

	public override void SetDefaults()
	{
		base.SetDefaults();

		// Combat.
		NPC.damage = 40;
		NPC.knockBackResist = 0.35f;
		NPC.defense = 20;
		NPC.lifeMax = NPC.life = 100;
		// Universal.
		NPC.color = Color.White;
		NPC.alpha = 0;
		NPC.value = 200f;
	}
}
#endif