using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Items;

public sealed class PlayerItemUse : ModPlayer
{
	public delegate void UseMiningToolDelegate(Player player, Item sItem, out bool canHitWalls, int x, int y);

	private static UseMiningToolDelegate? useMiningToolDelegate;

	private bool forceItemUse;
	private bool isItemUseForced;
	private int altFunctionUse;
	private ulong lastItemAnimationStartTime;

	public uint TimeSinceLastUseAnimation => (uint)Math.Max(0ul, TimeSystem.UpdateCount - lastItemAnimationStartTime);

	public override void Load()
	{
		useMiningToolDelegate = typeof(Player)
			.GetMethod("ItemCheck_UseMiningTools_ActuallyUseMiningTool", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?
			.CreateDelegate<UseMiningToolDelegate>()
			?? throw new InvalidOperationException("Unable to acquire mining tool usage method delegate.");

		On_Player.ApplyItemAnimation_Item_float_Nullable1 += static (orig, player, item, multiplier, itemReuseDelay) => {
			orig(player, item, multiplier, itemReuseDelay);

			if (player.TryGetModPlayer(out PlayerItemUse itemUse)) {
				itemUse.lastItemAnimationStartTime = TimeSystem.UpdateCount;
			}
		};
	}

	public override bool PreItemCheck()
	{
		if (forceItemUse) {
			Player.controlUseItem = true;
			Player.altFunctionUse = altFunctionUse;
			Player.itemAnimation = 0;
			Player.reuseDelay = 0;
			Player.itemTime = 0;

			isItemUseForced = true;
			forceItemUse = false;
		} else if (isItemUseForced) {
			if (!Player.IsLocal()) {
				Player.controlUseItem = false;
			}

			isItemUseForced = false;
		}

		return true;
	}

	public void ForceItemUse(int altFunctionUse = 0)
	{
		forceItemUse = true;
		this.altFunctionUse = altFunctionUse;
	}

	public static void UseMiningTool(Player player, Item sItem, out bool canHitWalls, int x, int y)
		=> useMiningToolDelegate!(player, sItem, out canHitWalls, x, y);
}
