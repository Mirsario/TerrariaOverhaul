using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public struct MovementModifier
{
	public static readonly MovementModifier Default = new();

	public float GravityScale = 1f;
	public float RunAccelerationScale = 1f;
	public (Vector2 Positive, Vector2 Negative) VelocityScale = (Vector2.One, Vector2.One);

	public MovementModifier() { }

	public void Apply(Player player)
	{
		player.gravity *= GravityScale;
		player.runAcceleration *= RunAccelerationScale;

		player.position -= Vector2.Max(player.oldVelocity, Vector2.Zero) * (Vector2.One - VelocityScale.Positive);
		player.position -= Vector2.Min(player.oldVelocity, Vector2.Zero) * (Vector2.One - VelocityScale.Negative);
	}

	public static MovementModifier Lerp(MovementModifier a, MovementModifier b, float step)
		=> Lerp(in a, in b, step);

	public static MovementModifier Lerp(in MovementModifier a, in MovementModifier b, float step)
	{
		MovementModifier result;

		result.GravityScale = MathHelper.Lerp(a.GravityScale, b.GravityScale, step);
		result.RunAccelerationScale = MathHelper.Lerp(a.RunAccelerationScale, b.RunAccelerationScale, step);
		result.VelocityScale = (
			Vector2.Lerp(a.VelocityScale.Positive, b.VelocityScale.Positive, step),
			Vector2.Lerp(a.VelocityScale.Negative, b.VelocityScale.Negative, step)
		);

		return result;
	}
}
