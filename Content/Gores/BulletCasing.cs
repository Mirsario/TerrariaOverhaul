using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Gores;

namespace TerrariaOverhaul.Content.Gores
{
	public class BulletCasing : ModGore
	{
		public static readonly ModSoundStyle BounceSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/CasingBounce", 4, volume: 0.085f, pitchVariance: 0.2f);

		public override void OnSpawn(Gore gore, IEntitySource source)
		{
			gore.Frame = new SpriteFrame(1, 1, 0, 0);
			gore.sticky = false;
			gore.light = 0f;
			gore.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			gore.scale = 0.7f;
		}

		public override bool Update(Gore gore)
		{
			if (gore is OverhaulGore goreExt) {
				goreExt.customBounceSound = BounceSound;
			}

			return base.Update(gore);
		}
	}
}
