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
using TerrariaOverhaul.Core.Localization;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.MainMenuOverlays;

public class MenuLine
{
	public Vector2 Size { get; set; }
	public Text Text { get; set; }
	public float Scale { get; set; } = 1f;
	public Asset<DynamicSpriteFont> Font { get; set; } = FontAssets.MouseText;
	public Func<bool, Color>? ForcedColor { get; set; }

	protected bool IsHovered { get; private set; }

	public virtual bool IsActive => true;

	public MenuLine(Text text)
	{
		Text = text;
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
