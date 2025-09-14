// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Footsteps;

internal sealed class PlayerFootsteps : ModPlayer
{
	public static readonly ConfigEntry<bool> EnablePlayerFootsteps = new(ConfigSide.ClientOnly, true, "Ambience");

	private const double FootstepCooldown = 0.1;

	private byte stepState;
	private double lastFootstepTime;
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
			double time = TimeSystem.LogicTime;

			if (time - lastFootstepTime > FootstepCooldown && FootstepSystem.Footstep(Player, footstepType.Value)) {
				stepState = (byte)(stepState == 0 ? 1 : 0);
				lastFootstepTime = TimeSystem.LogicTime;
			}
		}

		if (!onGround || legFrame == 0) {
			stepState = 0;
		}
	}
}
