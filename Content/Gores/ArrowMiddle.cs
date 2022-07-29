using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Gores;

public class ArrowMiddle : ModGore
{
	public override void SetStaticDefaults()
	{
		ChildSafety.SafeGore[Type] = true;
	}

	public override void OnSpawn(Gore gore, IEntitySource source)
	{
		gore.Frame = new SpriteFrame(2, 1, (byte)Main.rand.Next(2), 0);
		gore.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
		gore.scale = Main.rand.NextFloat(0.9f, 1f);
		gore.drawOffset = new Vector2(0f, 4f);
	}
}
