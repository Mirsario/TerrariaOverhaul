using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Gores
{
	public class GenericGore : ModGore
	{
		public override void OnSpawn(Gore gore)
		{
			gore.Frame = new SpriteFrame(0, 3);
			gore.sticky = false;
			gore.light = 0f;
			gore.drawOffset = new Vector2(0f, 4f);
			gore.rotation = Main.rand.NextFloat(0f, MathHelper.TwoPi);
		}
	}
}
