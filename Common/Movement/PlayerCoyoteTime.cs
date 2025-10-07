using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Movement;

internal sealed class PlayerCoyoteTime : ModPlayer
{
    private float coyoteTimer;
    private const float CoyoteDuration = 0.3f;

    public override void PostUpdate()
    {
        // Start the coyote timer when the player *leaves* the ground. Setting it while
        // the player is grounded would make the window active at the wrong time.
        if (Player.oldVelocity.Y == 0f && Player.velocity.Y != 0f) {
            coyoteTimer = CoyoteDuration;
        } else {
            coyoteTimer = MathUtils.StepTowards(coyoteTimer, 0f, TimeSystem.LogicDeltaTime);
        }
    }

    public override void Load()
    {
        On_Player.JumpMovement += JumpMovementHook;
    }

    private static void JumpMovementHook(On_Player.orig_JumpMovement orig, Player player)
{
    var modPlayer = player.GetModPlayer<PlayerCoyoteTime>();

    // Only allow coyote jump if the player is airborne, the coyote window is active,
    // and the player is actually pressing the jump key.
    bool allowCoyote = player.velocity.Y != 0f && modPlayer.coyoteTimer > 0f && player.controlJump;

    if (allowCoyote)
    {
        modPlayer.coyoteTimer = 0f;

        float originalVelX = player.velocity.X;

        float jumpVel = (0f - Player.jumpSpeed) * player.gravDir;
        player.velocity = new Microsoft.Xna.Framework.Vector2(originalVelX, jumpVel);

        player.releaseJump = false;
        if (player.jump <= 0) {
            player.jump = 1;
        }

    }
    else
    {
        orig(player);
    }
}

}
