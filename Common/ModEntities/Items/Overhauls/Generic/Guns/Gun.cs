using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public abstract class Gun : AdvancedItem
	{
		public void SpawnCasings<T>(Player player, int amount = 1) where T : ModGore
		{
			var position = player.Center + new Vector2(player.direction > 0 ? 0f : -6f, -12f);

			for(int i = 0; i < amount; i++) {
				var velocity = player.velocity * 0.5f + new Vector2(Main.rand.NextFloat(1f) * -player.direction, Main.rand.NextFloat(-0.5f, -1.5f));

				Gore.NewGore(position, velocity, ModContent.GoreType<T>());
			}
		}
	}
}
