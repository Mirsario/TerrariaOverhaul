using Terraria;

namespace TerrariaOverhaul.Common.Movement;

public struct MovementModifier
{
	public float GravityScale = 1f;
	public float RunAccelerationScale = 1f;

	public MovementModifier() { }

	public void Apply(Player player)
	{
		player.gravity *= GravityScale;
		player.runAcceleration *= RunAccelerationScale;
	}
}
