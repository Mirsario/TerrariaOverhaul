// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;

namespace TerrariaOverhaul.Utilities.Terraria;

internal sealed class NpcTracker(NPC npc)
{
	private readonly int type = npc.type;
	private readonly int index = npc.whoAmI;

	public NPC? Npc() => (!Main.gameMenu && Main.npc[index] is NPC { active: true } npc && npc.type == type) ? npc : null;
	public Vector2? Center() => Npc()?.Center;
	
	public bool AudioCallback(ActiveSound sound)
	{
		if (Npc() is not NPC npc)
			return false;

		sound.Position = npc.Center;
		return true;
	}
}
