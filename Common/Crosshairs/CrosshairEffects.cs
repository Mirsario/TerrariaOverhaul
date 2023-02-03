using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Common.Crosshairs;

public struct CrosshairEffects
{
	public (float Value, float LengthFactor) Offset;
	public (float Value, float LengthFactor) Rotation;
	public (Color Value, float LengthFactor) InnerColor;
	public (Color Value, float LengthFactor) OuterColor;
}
