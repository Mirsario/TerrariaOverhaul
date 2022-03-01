using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using TerrariaOverhaul.Core.Input;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.MainMenuOverlays
{
	public class MenuLine
	{
		public readonly string Text;
		public readonly float Scale;
		public readonly Asset<DynamicSpriteFont> Font;
		public readonly Vector2 Size;

		protected readonly Func<bool, Color> ForcedColor;

		protected bool IsHovered { get; private set; }

		public MenuLine(string text, Asset<DynamicSpriteFont> font = null, float scale = 1f, Func<bool, Color> forcedColor = null)
		{
			Text = text;
			Font = font ?? FontAssets.MouseText;
			Scale = scale;
			ForcedColor = forcedColor;

			Size = Font.Value.MeasureString(Text) * Scale;
		}

		protected virtual void OnClicked() { }

		public virtual void Update(Vector2 position)
		{
			var rect = new Rectangle((int)position.X, (int)position.Y, (int)Size.X, (int)Size.Y);

			IsHovered = rect.Contains(new Vector2Int(Main.mouseX, Main.mouseY));

			if (IsHovered && InputSystem.GetMouseButtonDown(0)) {
				SoundEngine.PlaySound(SoundID.MenuOpen);
				OnClicked();
			}
		}

		public virtual void Draw(SpriteBatch sb, Vector2 position)
		{
			var color = ForcedColor?.Invoke(IsHovered) ?? Color.White;

			sb.DrawStringOutlined(Font.Value, Text, position, color);
		}
	}
}
