// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrariaOverhaul.Utilities.Xna;

internal struct VertexPositionUv3(Vector3 position, Vector2 uv0, Vector2 uv1, Vector2 uv2) : IVertexType
{
	public static readonly VertexDeclaration VertexDeclaration = new(
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
		new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
	);

	public Vector3 Position = position;
	public Vector2 Uv0 = uv0;
	public Vector2 Uv1 = uv1;
	public Vector2 Uv2 = uv2;

	readonly VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
}
