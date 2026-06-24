// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Footsteps;

internal sealed class PlayerFootsteps : ModPlayer
{
        public static readonly ConfigEntry<bool> EnablePlayerFootsteps = new(ConfigSide.ClientOnly, true, "Ambience");

        private const uint FootstepCooldown = 5;

        private byte stepState;
        private uint lastNormalStepTick;
        private uint lastSpecialStepTick;
        private bool bouncedThisFrame;

        public override void Load()
        {
                IL_Player.TryBouncingBlocks += TryBouncingBlocksInjection;
        }

        public override void PostItemCheck()
        {
                UpdateFootsteps();
                bouncedThisFrame = false;
        }

        private static void TryBouncingBlocksInjection(ILContext context)
        {
                var il = new ILCursor(context);

                // Match before 'if (controlJump)'
                il.GotoNext(
                        MoveType.Before
                        , i => i.MatchLdarg0()
                        , i => i.MatchLdfld(typeof(Player), nameof(Player.controlJump))
                );
                il.Index++;
                ILUtils.HijackIncomingLabels(il);

                il.Emit(OpCodes.Ldarg_0);
                il.EmitDelegate(static (Player p) => {
                        if (p.TryGetModPlayer(out PlayerFootsteps footsteps))
                                footsteps.bouncedThisFrame = true;
                });
        }

        private void UpdateFootsteps()
        {
                if (Main.dedServ || !EnablePlayerFootsteps) {
                        return;
                }

                // Suppress footsteps while climbing ropes, using Overhaul climbing, or grappling
                if (Player.pulley || Player.GetModPlayer<PlayerClimbing>().IsClimbing || Player.EnumerateGrapplingHooks().Any()) {
                        return;
                }

                bool onGround = Player.OnGround();
                bool wasOnGround = Player.WasOnGround();
                int legFrame = Player.legFrame.Y / Player.legFrame.Height;

                FootstepType? footstepType = null;

                if (onGround != wasOnGround || bouncedThisFrame) {
                        if (!onGround || Player.controlJump) {
                                footstepType = FootstepType.Jump;
                        } else {
                                footstepType = FootstepType.Land;
                        }
                } else if (onGround) {
                        footstepType = FootstepType.Default;
                }

                if (footstepType.HasValue && (footstepType.Value != FootstepType.Default || stepState == 1 && (legFrame == 16 || legFrame == 17) || stepState == 0 && (legFrame == 9 || legFrame == 10))) {
                        uint tick = Main.GameUpdateCount;
                        ref uint lastStep = ref (footstepType.Value == FootstepType.Default ? ref lastNormalStepTick : ref lastSpecialStepTick);
                        var movement = Player.GetModPlayer<PlayerMovement>();

                        if ((tick - lastStep) > FootstepCooldown && FootstepSystem.Footstep(new() {
                                Kind = footstepType.Value,
                                Hitbox = Player.Hitbox,
                                Velocity = (Player.velocity, movement.SelectVelocity(3, (a, b) => a.Y > b.Y)),
                        })) {
                                stepState = (byte)(stepState == 0 ? 1 : 0);
                                lastStep = tick;
                        }
                }

                if (!onGround || legFrame == 0) {
                        stepState = 0;
                }
        }
}
