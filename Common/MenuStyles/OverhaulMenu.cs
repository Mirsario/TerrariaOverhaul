using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.MenuStyles
{
	public sealed class OverhaulMenu : ModMenu
	{
		public override Asset<Texture2D> Logo => Mod.Assets.Request<Texture2D>("Common/MenuStyles/Logo");
	}
}
