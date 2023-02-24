using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerJumpBuffering : ModPlayer
{
	public float JumpKeyBuffer { get; private set; }

	public override void Load()
	{
		On_Player.JumpMovement += JumpMovement;
	}

	public override void PostUpdate()
	{
		JumpKeyBuffer = MathUtils.StepTowards(JumpKeyBuffer, 0f, TimeSystem.LogicDeltaTime);
	}

	public override void SetControls()
	{
		if (Player.controlJump && Player.releaseJump && Player.velocity.Y != 0f) {
			JumpKeyBuffer = 0.25f;
		}
	}

	private static void JumpMovement(On_Player.orig_JumpMovement orig, Player player)
	{
		var modPlayer = player.GetModPlayer<PlayerJumpBuffering>();
		bool forceJump = !player.controlJump && modPlayer.JumpKeyBuffer > 0f && player.velocity.Y == 0f;
		bool originalControlJump = player.controlJump;
		bool originalAutoJump = player.autoJump;

		if (forceJump) {
			modPlayer.JumpKeyBuffer = 0f;

			player.controlJump = player.autoJump = true;
		}

		orig(player);

		if (forceJump) {
			player.controlJump = originalControlJump;
			player.autoJump = originalAutoJump;
		}
	}
}
