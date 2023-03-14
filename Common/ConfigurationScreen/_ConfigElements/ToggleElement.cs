using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class ToggleElement : UIElement
{
	private static Asset<Texture2D>? toggleTexture;

	public bool Value { get; set; }

	public override void OnInitialize()
	{
		base.OnInitialize();
	}

	public override void Recalculate()
	{
		base.Recalculate();
	}

	public override void MouseDown(UIMouseEvent evt)
	{
		base.MouseDown(evt);

		Value = !Value;
	}

	protected override void DrawSelf(SpriteBatch sb)
	{
		base.DrawSelf(sb);

		toggleTexture ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/Toggle");

		if (toggleTexture is not { IsLoaded: true, Value: Texture2D toggleTextureValue }) {
			return;
		}

		CalculatedStyle dimensions = GetDimensions();

		bool value = Value;
		
		// Texture

		var toggleSrcRect = new Rectangle(
			0,
			value ? toggleTextureValue.Height / 2 : 0,
			toggleTextureValue.Width,
			toggleTextureValue.Height / 2
		);
		var togglePosition = new Vector2(
			dimensions.X + dimensions.Width - toggleSrcRect.Width + 8f,
			dimensions.Y - 8f
		);
		
		sb.Draw(toggleTextureValue, togglePosition, toggleSrcRect, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

		// Text

		var textFont = FontAssets.ItemStack.Value;
		string textOn = "On";
		string textOff = "Off";
		var textOriginOn = textFont.MeasureString(textOn) * 0.5f;
		var textOriginOff = textFont.MeasureString(textOff) * 0.5f;
		var textColorOn = value ? Color.White : Color.Gray;
		var textColorOff = value ? Color.Gray : Color.White;
		var textPositionOn = togglePosition + new Vector2(toggleSrcRect.Width - 36f, 19f) - textOriginOn;
		var textPositionOff = togglePosition + new Vector2(36f, 19f) - textOriginOff;

		ChatManager.DrawColorCodedStringWithShadow(sb, textFont, textOn, textPositionOn, textColorOn, 0f, default, new Vector2(1.0f));
		ChatManager.DrawColorCodedStringWithShadow(sb, textFont, textOff, textPositionOff, textColorOff, 0f, default, new Vector2(1.0f));
	}
}
