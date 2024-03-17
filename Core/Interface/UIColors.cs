using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Core.Interface;

public record struct UIColors(Color Normal, Color? Hover = null, Color? Active = null);
