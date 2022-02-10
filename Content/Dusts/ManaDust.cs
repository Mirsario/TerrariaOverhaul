using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Content.Dusts
{
	public class ManaDust : ModDust
	{
		public override void OnSpawn(Dust dust, IEntitySource source)
		{
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 22, 22);
			dust.scale = 0.5f;
		}

		public override bool Update(Dust dust)
		{
			dust.velocity.Y -= 1f / 60f;
			dust.fadeIn += 1f / 60f;

			return true;
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return Color.White.WithAlpha(0.5f);
		}
	}
}
