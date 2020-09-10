using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Players;

namespace TerrariaOverhaul.Common.ModEntities.Items
{
	public sealed class ItemPlayers : GlobalItem
	{
		public override bool CanUseItem(Item item,Player player) => player.GetModPlayer<PlayerItems>().canUseItems;
	}
}
