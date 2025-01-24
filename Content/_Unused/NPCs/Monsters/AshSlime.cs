// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

// Unused content, may be reintroduced in the future.
#if false
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace TerrariaOverhaul.Content.NPCs.Monsters.TheAshes;

public class AshSlime : AshMonster
{
	protected override int BaseNPC => NPCID.BlueSlime;

	public override void SetDefaults()
	{
		base.SetDefaults();

		// Combat.
		NPC.damage = 50;
		NPC.knockBackResist = 0.4f;
		NPC.defense = 20;
		NPC.lifeMax = NPC.life = 75;
		// Universal.
		NPC.width = 40;
		NPC.height = 27;
		NPC.color = Color.White;
		NPC.alpha = 0;
		NPC.value = 200f;
	}
}
#endif
