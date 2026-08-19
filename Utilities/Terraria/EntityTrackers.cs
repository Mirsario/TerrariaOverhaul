// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;

namespace TerrariaOverhaul.Utilities.Terraria;

internal readonly struct NpcTracker(NPC npc)
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

internal readonly struct ProjectileTracker(Projectile projectile)
{
	private readonly int type = projectile.type;
	private readonly int index = projectile.whoAmI;

	public Projectile? Projectile() => (!Main.gameMenu && Main.projectile[index] is Projectile { active: true } proj && proj.type == type) ? proj : null;
	public Vector2? Center() => Projectile()?.Center;
	
	public bool AudioCallback(ActiveSound sound)
	{
		if (Projectile() is not Projectile proj)
			return false;

		sound.Position = proj.Center;
		return true;
	}
}

internal readonly struct PlayerTracker(Player player)
{
	private readonly int nameHash = player.name.GetHashCode();
	private readonly int index = player.whoAmI;

	public Player? Get() => (!Main.gameMenu && Main.player[index] is Player { active: true } player && player.name.GetHashCode() == nameHash) ? player : null;
	public Vector2? Center() => Get()?.Center;

	public bool AudioCallback(ActiveSound sound)
	{
		if (Get() is not Player player)
			return false;

		sound.Position = player.Center;
		return true;
	}
}

internal readonly struct HeldItemTracker(Player player)
{
	private readonly PlayerTracker playerTracker = new(player);
	private readonly int itemIndex = player.selectedItem;
	private readonly int itemType = player.HeldItem.type;

	public (Player Player, Item Item)? Get()
	{
		if (playerTracker.Get() is not Player { } player) return null;
		if (player.selectedItem != itemIndex) return null;
		if (player.HeldItem is not { } item) return null;
		if (item.type != itemType) return null;
		return (player, item);
	}

	public bool AudioCallback(ActiveSound sound)
	{
		if (Get() is not { Player: { } player })
			return false;

		sound.Position = player.Center;
		return true;
	}
}
