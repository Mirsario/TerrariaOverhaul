using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Interface;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public class FancyUIPanel : UIPanel
{
	private static Asset<Texture2D>? panelBorder;
	private static Asset<Texture2D>? panelBackground;

	private static Asset<Texture2D> PanelBorder
		=> panelBorder ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/PanelBorder");

	private static Asset<Texture2D> PanelBackground
		=> panelBackground ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/UI/PanelBackground");

	// Components
	public DynamicColorsUIComponent Colors { get; }
	public SoundPlaybackUIComponent SoundPlayback { get; }

	public FancyUIPanel() : base(PanelBackground, PanelBorder)
	{
		Colors = this.AddComponent(new DynamicColorsUIComponent());
		SoundPlayback = this.AddComponent(new SoundPlaybackUIComponent());
	}
}
