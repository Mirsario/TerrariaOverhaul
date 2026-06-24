// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Interaction;

internal sealed class InteractionKeySystem : ModSystem
{
	public static ModKeybind InteractKey { get; private set; }

	public override void Load()
	{
		InteractKey = KeybindLoader.RegisterKeybind(Mod, "Interact", "MouseRight");
		On_Player.ItemCheck += ItemCheckDetour;
	}

	public override void Unload()
	{
		InteractKey = null;
	}

	private static void ItemCheckDetour(On_Player.orig_ItemCheck orig, Player player)
	{
		if (!player.IsLocal()) {
			orig(player);
			return;
		}

		// Check if player remapped interact to a different key
		bool interactKeyIsDifferent = IsInteractKeyDifferentFromRightClick();

		// Use vanilla hover detection instead of hardcoded tile list
		bool isHoveringInteractable = Main.SmartInteractShowingGenuine ||
									  player.noThrow > 0 ||
									  Main.HoveringOverAnNPC ||
									  Main.tileSignHovered;

		// 1. If dedicated interact key is pressed, force vanilla interaction and block power attack
		if (InteractKey.Current) {
			player.controlUseTile = true;
			ItemPowerAttacks.BlockPowerAttackForFrame = true;
		}
		// 2. If Right Click is pressed but it's NOT the interact key, suppress vanilla interactions (pure weapon use)
		else if (player.controlUseTile && interactKeyIsDifferent) {
			player.controlUseTile = false;
		}
		// 3. Fallback: Same keybind (MouseRight) but hovering over something -> suppress weapon
		else if (player.controlUseTile && isHoveringInteractable) {
			ItemPowerAttacks.BlockPowerAttackForFrame = true;
		}

		orig(player);
	}

	public static bool IsInteractKeyDifferentFromRightClick()
	{
		var interactKeys = InteractKey.GetAssignedKeys();
		if (interactKeys.Length != 1) {
			return interactKeys.Length > 0;
		}
		return interactKeys[0] != "MouseRight";
	}
}
