using Terraria.ModLoader;
using Hook = TerrariaOverhaul.Common.Hooks.Items.IModifyItemUseSound;

namespace TerrariaOverhaul.Common.Hooks.Items
{
	internal sealed class ModifyItemUseSoundImplementation : GlobalItem
	{
		public override void Load()
		{
			On.Terraria.Player.ItemCheck_StartActualUse += (orig, player, item) => {
				var heldItem = player.HeldItem;

				if (heldItem == null || heldItem.IsAir) {
					orig(player, item);
					return;
				}

				var useSoundBackup = heldItem.UseSound;

				Hook.Invoke(heldItem, player, ref heldItem.UseSound);

				bool soundSwapped = heldItem.UseSound != useSoundBackup;

				try {
					orig(player, item);
				}
				finally {
					if (soundSwapped) {
						heldItem.UseSound = useSoundBackup;
					}
				}
			};
		}
	}
}
