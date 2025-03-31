// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrariaOverhaul.Utilities.Xna;

public struct VertexPositionUv2(Vector3 position, Vector2 uv0, Vector2 uv1) : IVertexType
{
	public static readonly VertexDeclaration VertexDeclaration = new(
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
	);

	public Vector3 Position = position;
	public Vector2 Uv0 = uv0;
	public Vector2 Uv1 = uv1;

	readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}
