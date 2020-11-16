using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items
{
	public class ItemAutoSwing : GlobalItem
	{
		public override void SetDefaults(Item item)
		{
			if(!item.autoReuse) {
				item.autoReuse = true;
				item.useTime += 2;
				item.useAnimation += 2;
			}
		}
	}
}
