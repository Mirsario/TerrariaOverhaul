using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Xna;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Movement;

internal sealed class PlayerCoyoteTime : ModPlayer
{
    public static readonly ConfigEntry<bool> EnableCoyoteTime = new(ConfigSide.ClientOnly, true, "Movement");

    private float coyoteTimer;
    private const float CoyoteDuration = 0.15f;
    private const int ForcedJumpHoldAmount = 10;

    public override void PostUpdate()
    {
        if (!EnableCoyoteTime) {
            return;
        }
        // Start the coyote timer when the player starts falling off a ledge.
        if (Player.oldVelocity.Y == 0f && Player.velocity.Y > 0f) {
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
    if (!EnableCoyoteTime) {
        orig(player);
        return;
    }
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

        // Make the coyote jump behave more like a held jump
        player.releaseJump = false;
        player.jump = System.Math.Max(player.jump, ForcedJumpHoldAmount);

    }
    else
    {
        orig(player);
    }
}

}
