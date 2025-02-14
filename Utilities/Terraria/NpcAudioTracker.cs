// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.Audio;

namespace TerrariaOverhaul.Utilities.Terraria;

public sealed class NpcAudioTracker(NPC npc, bool trackCenter)
{
	private readonly int type = npc.type;
	private readonly int index = npc.whoAmI;

	public bool Callback(ActiveSound sound)
	{
		if (Main.gameMenu)
			return false;

		if (Main.npc[index] is not NPC { active: true } npc || npc.type != type)
			return false;

		if (trackCenter)
			sound.Position = npc.Center;

		return true;
	}
}
