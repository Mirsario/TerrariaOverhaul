using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;

namespace TerrariaOverhaul.Content.Gores
{
	public class GenericGore : ModGore
	{
		public override void OnSpawn(Gore gore, IEntitySource source)
		{
			gore.Frame = new SpriteFrame(1, 3, 0, (byte)Main.rand.Next(3));
			gore.sticky = false;
			gore.light = 0f;
			gore.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			gore.drawOffset = new Vector2(0f, 4f);
		}

		public override Color? GetAlpha(Gore gore, Color lightColor) => (gore as OverhaulGore)?.bleedColor?.MultiplyRGB(lightColor);
	}
}
