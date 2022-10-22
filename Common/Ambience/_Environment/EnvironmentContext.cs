using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Common.Ambience;

public readonly ref struct EnvironmentContext
{
	public Player Player { get; init; }
	public Vector2 PlayerTilePosition { get; init; }
	public ReadOnlySpan<int> TileCounts { get; init; }
	public SceneMetrics Metrics { get; init; }
}
