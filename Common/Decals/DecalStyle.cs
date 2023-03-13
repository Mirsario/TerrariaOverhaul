using Microsoft.Xna.Framework.Graphics;

namespace TerrariaOverhaul.Common.Decals;

public sealed class DecalStyle
{
	public static DecalStyle Default { get; } = new DecalStyle(BlendState.AlphaBlend);
	public static DecalStyle Opaque { get; } = new DecalStyle(BlendState.Opaque);
	public static DecalStyle Subtractive { get; } = new DecalStyle(new BlendState() {
		ColorSourceBlend = Blend.One,
		ColorDestinationBlend = Blend.One,
		ColorBlendFunction = BlendFunction.ReverseSubtract,
		AlphaSourceBlend = Blend.One,
		AlphaDestinationBlend = Blend.One,
		AlphaBlendFunction = BlendFunction.ReverseSubtract,
	});

	public BlendState BlendState { get; }

	internal int Id { get; set; } = -1;

	public DecalStyle(BlendState blendState)
	{
		BlendState = blendState;
	}

	internal static void RegisterDefaultStyles()
	{
		DecalSystem.RegisterStyle(Default);
		DecalSystem.RegisterStyle(Opaque);
		DecalSystem.RegisterStyle(Subtractive);
	}
}
