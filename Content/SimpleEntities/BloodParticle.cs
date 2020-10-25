using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Content.SimpleEntities
{
	[Autoload(Side = ModSide.Client)]
	public class BloodParticle : Particle
	{
		private static Asset<Texture2D> texture;

		private Rectangle frame;

		//Load-time
		public override void Load(Mod mod) => texture = ModContent.GetTexture(ModUtils.GetTypePath(GetType()));
		public override void Unload() => texture = null;
		//In-game
		public override void Init()
		{
			frame = new Rectangle(0, 8 * Main.rand.Next(3), 8, 8);
			gravity = new Vector2(0f, 300f);
		}
		public override void Draw(SpriteBatch sb)
		{
			var source = frame;
			var dest = new Rectangle(
				(int)(position.X - Main.screenPosition.X - source.Width * 0.5f * (scale.X - 1f)),
				(int)(position.Y - Main.screenPosition.Y - source.Height * 0.5f * (scale.Y - 1f)),
				(int)(source.Width * scale.X),
				(int)(source.Height * scale.Y)
			);

			var origin = source.Size() * 0.5f;
			var usedColor = Lighting.GetColor((int)(position.X / 16f), (int)(position.Y / 16f), color.WithAlpha((byte)(color.A * alpha)));

			sb.Draw(texture.Value, dest, source, usedColor, rotation, origin, SpriteEffects.None, 0f);
		}
	}
}
