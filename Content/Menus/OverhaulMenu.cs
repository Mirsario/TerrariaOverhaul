using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Menus
{
	public sealed class OverhaulMenu : ModMenu
	{
		private Asset<Texture2D> texture;

		public override Asset<Texture2D> Logo => texture;

		public override void Load()
		{
			texture = Mod.Assets.Request<Texture2D>("Logo");
		}
	}
}
