using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaOverhaul.Core.Time;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

[Autoload(Side = ModSide.Client)]
public sealed class UIConfigIcon : UIElement, ILoadable
{
	private static readonly RasterizerState OverflowHiddenRasterizerState = new() {
		CullMode = CullMode.None,
		ScissorTestEnable = true
	};
	private static Asset<Effect>? shader;
	//private static SpriteBatch? personalSb;
	//private static bool sbBegan;
	//private static Matrix lastMatrix;

	public Color Color = Color.White;
	public Vector2 NormalizedOrigin = Vector2.Zero;
	public Vector2? ResolutionOverride;

	public Asset<Texture2D> ForegroundTexture { get; set; }
	public Asset<Texture2D> BackgroundTexture { get; set; }

	private UIConfigIcon()
	{
		ForegroundTexture = null!;
		BackgroundTexture = null!;
	}

	public UIConfigIcon(Asset<Texture2D> foreground, Asset<Texture2D> background)
	{
		ForegroundTexture = foreground;
		BackgroundTexture = background;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (ForegroundTexture.Value is not Texture2D foreground
		|| BackgroundTexture?.Value is not Texture2D background
		|| shader?.Value is not Effect effect) {
			return;
		}

		// Configure effect.
		effect.Parameters["Time"]?.SetValue(TimeSystem.RenderTime);
		effect.Parameters["Resolution"]?.SetValue(ResolutionOverride ?? foreground.Size());
		effect.Parameters["Background"]?.SetValue(background);
		//effect.Parameters["OutlineColor"]?.SetValue(Main.DiscoColor.ToVector4());

		var dimensions = GetDimensions();
		var rect = dimensions.ToRectangle();
		//var matrix = Main.UIScaleMatrix;
		//bool matrixDiffers = matrix != lastMatrix;

		//if (!sbBegan || matrixDiffers) {
			//if (sbBegan) {
				//personalSb!.End();
			//}

			//personalSb!.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, OverflowHiddenRasterizerState, effect, matrix);
			//sbBegan = true;
			//lastMatrix = matrix;
		//}

		//personalSb!.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, OverflowHiddenRasterizerState, effect, matrix);
		//personalSb!.Draw(foreground, rect, Color);
		//personalSb!.End();

		// This is very slow, but this will hopefully only happen once per frame.
		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, OverflowHiddenRasterizerState, effect, Main.UIScaleMatrix);

		spriteBatch.Draw(foreground, rect, Color);

		spriteBatch.End();
		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);
	}

	void ILoadable.Load(Mod mod)
	{
		shader = mod.Assets.Request<Effect>($"Assets/Shaders/ConfigIcon");

		//Main.QueueMainThreadAction(static () => personalSb = new SpriteBatch(Main.graphics.GraphicsDevice));
	}

	void ILoadable.Unload()
	{
		//personalSb?.Dispose();	
		shader = null;
		//personalSb = null;
	}
}
