using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrariaOverhaul.Core.DataStructures;

public struct VertexPositionUv2 : IVertexType
{
	public static readonly VertexDeclaration VertexDeclaration = new(
		new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
		new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
		new VertexElement(20, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
	);

	public Vector3 position;
	public Vector2 uv0;
	public Vector2 uv1;

	VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

	public VertexPositionUv2(Vector3 position, Vector2 uv0, Vector2 uv1)
	{
		this.position = position;
		this.uv0 = uv0;
		this.uv1 = uv1;
	}
}
