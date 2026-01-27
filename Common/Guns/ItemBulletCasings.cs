// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Guns;

[Autoload(Side = ModSide.Client)]
internal sealed class ItemBulletCasings : ItemComponent
{
	public static readonly ConfigEntry<bool> EnableBulletCasings = new(ConfigSide.ClientOnly, true, "Guns");

	public int CasingCount = 1;
	public int CasingGoreType = -1;
	public bool SpawnOnUse = true;

	private new bool Enabled => base.Enabled && EnableBulletCasings;

	public override bool? UseItem(Item item, Player player)
	{
		if (Enabled && SpawnOnUse) {
			SpawnCasings(item, player);
		}

		return base.UseItem(item, player);
	}

	public void SpawnCasings(Item item, Player player, int? amountOverride = null)
	{
		if (CasingGoreType >= 0) {
			SpawnCasings(item, player, CasingGoreType, amountOverride ?? CasingCount);
		}
	}

	public static void SpawnCasings<T>(Item item, Player player, int amount = 1) where T : ModGore
		=> SpawnCasings(item, player, ModContent.GoreType<T>(), amount);

	public static void SpawnCasings(Item item, Player player, int casingGoreType, int amount = 1)
	{
		var position = player.Center + new Vector2(player.direction > 0 ? 0f : -6f, -12f);
		var entitySource = player.GetSource_ItemUse(item);

		for (int i = 0; i < amount; i++) {
			var velocity = player.velocity * 0.5f + new Vector2(Main.rand.NextFloat(1f) * -player.direction, Main.rand.NextFloat(-0.5f, -1.5f));

			Gore.NewGore(entitySource, position, velocity, casingGoreType);
		}
	}
}
