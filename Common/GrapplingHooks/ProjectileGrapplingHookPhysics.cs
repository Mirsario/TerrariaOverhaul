// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.GrapplingHooks;

// Could use more comments.
internal class ProjectileGrapplingHookPhysics : GlobalProjectile
{
	public static readonly ConfigEntry<bool> EnableGrapplingHookPhysics = new(ConfigSide.Both, true, "Movement");

	public const int GrapplingHookAIStyle = ProjAIStyleID.Hook;

	private float maxDist;
	private bool noPulling;

	public override bool InstancePerEntity => true;

	public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation)
		=> lateInstantiation && projectile.aiStyle == GrapplingHookAIStyle;

	public override void Load()
	{
		IL_Player.Update += PlayerUpdateInjection;
		IL_Player.GrappleMovement += PlayerGrappleMovementInjection;

		On_Player.JumpMovement += (orig, player) => {
			if (ShouldOverrideGrapplingHookPhysics(player, out _)) {
				PlayerJumpOffGrapplingHook(player);
			}

			orig(player);
		};

		On_Projectile.AI_007_GrapplingHooks += static (orig, projectile) => {
			orig(projectile);

			if (!ShouldOverrideGrapplingHookPhysics(projectile, out var player)) {
				return;
			}

			if (!projectile.TryGetGlobalProjectile(out ProjectileGrapplingHookPhysics physics)) {
				return;
			}

			physics.ProjectileGrappleMovement(player, projectile);
		};
	}

	public void ProjectileGrappleMovement(Player player, Projectile proj)
	{
		var playerCenter = player.Center;
		var projCenter = proj.Center;
		var hookStats = GrapplingHookStats.Get(player, proj);

		var mountedCenter = player.MountedCenter;
		var mountedOffset = mountedCenter - projCenter;

		proj.rotation = (float)Math.Atan2(mountedOffset.Y, mountedOffset.X) - 1.57f;
		proj.velocity = Vector2.Zero;

		// If the grappling button is ever released - never allow pulling in again.
		if (!player.controlHook) {
			noPulling = true;
		}

		// 
		var dir = (playerCenter - projCenter).SafeNormalize(default);
		bool pull = !noPulling && player.controlHook;

		player.GoingDownWithGrapple = pull && dir.Y < 0f;

		// Prevent hooks from going farther than normal
		float ClampDistance(float distance)
			=> MathHelper.Clamp(distance, 0f, hookStats.Range - 1f);

		float dist = ClampDistance(Vector2.Distance(playerCenter, projCenter));
		bool down = player.controlDown && maxDist < hookStats.Range;
		bool up = player.controlUp;

		const float PullSpeed = 12.5f;
		const float PullVelocity = 0.1f;
		const float RaiseSpeed = 5f;
		const float RaiseVelocity = 0.975f;
		const float LowerSpeed = 5f;
		const float LowerVelocity = 1f;

		if (pull || (up || down) && up != down) {
			maxDist = dist = ClampDistance(dist + (pull ? -PullSpeed : up ? -RaiseSpeed : LowerSpeed));

			player.velocity *= pull ? PullVelocity : up ? RaiseVelocity : LowerVelocity;
		}

		float nextDistance = Vector2.Distance(playerCenter + player.velocity, projCenter);
		float deltaDistance = nextDistance - dist;
		var vect = projCenter - playerCenter;
		float vectLength = vect.Length();
		float speedPlusGravity = deltaDistance + player.gravity;
		float maxSpeed = Math.Max(dist / 10f, 12f);

		player.velocity = Vector2.Clamp(player.velocity, Vector2.One * -maxSpeed, Vector2.One * maxSpeed);

		if ((player.controlLeft || player.controlRight) && (!player.controlRight || !player.controlLeft)) {
			float accel = (player.controlLeft ? -6f : 6f) / 60f;

			player.velocity.X += accel;
			player.velocity = Vector2.Clamp(player.velocity, Vector2.One * -maxSpeed, Vector2.One * maxSpeed);
		} else {
			player.velocity.X *= 0.995f;
		}

		float tempVal;

		if (vectLength > speedPlusGravity) {
			tempVal = speedPlusGravity / vectLength;
		} else {
			tempVal = 1f;
		}

		vect *= tempVal;

		if (dist >= maxDist) {
			player.velocity += vect;
			player.maxRunSpeed = 15f;
			player.runAcceleration *= 3f;
		} else {
			player.runAcceleration = 0f;
			player.moveSpeed = 0f;
		}
	}

	public static void PlayerJumpOffGrapplingHook(Player player)
	{
		if (player.controlJump && player.releaseJump) {
			player.velocity.Y = Math.Min(player.velocity.Y, -Player.jumpSpeed);
			player.jump = 0;

			player.releaseJump = false;

			player.RefreshMovementAbilities();
			player.RemoveAllGrapplingHooks();
		}
	}

	public static bool GetHooked(Projectile proj)
		=> proj.ai[0] == 2f;

	public static void SetHooked(Projectile proj, bool newValue)
		=> proj.ai[0] = newValue ? 2f : 0f;

	public static bool ShouldOverrideGrapplingHookPhysics(Player? player)
		=> ShouldOverrideGrapplingHookPhysics(player, out _);

	public static bool ShouldOverrideGrapplingHookPhysics(Player? player, [NotNullWhen(true)] out Projectile? projectile)
	{
		int firstGrapple = player?.grappling[0] ?? -1;
		
		projectile = firstGrapple >= 0 ? Main.projectile[firstGrapple] : null;

		return ShouldOverrideGrapplingHookPhysics(player, projectile);
	}

	public static bool ShouldOverrideGrapplingHookPhysics(Projectile? projectile, [NotNullWhen(true)] out Player? player)
	{
		player = projectile?.GetOwner()!;

		return ShouldOverrideGrapplingHookPhysics(player, projectile);
	}

	private static readonly ContentSet NoGrapplingHookSwinging = nameof(NoGrapplingHookSwinging);

	public static bool ShouldOverrideGrapplingHookPhysics([NotNullWhen(true)] Player? player, [NotNullWhen(true)] Projectile? proj)
	{
		if (!EnableGrapplingHookPhysics) {
			return false;
		}

		if (player?.active != true || proj?.active != true) {
			return false;
		}

		if (proj.aiStyle != GrapplingHookAIStyle || !GetHooked(proj) || NoGrapplingHookSwinging.Has(proj)) {
			return false;
		}

		// Ignore fake minecart hooks.
		if (proj.type == ProjectileID.TrackHook) {
			return false;
		}

		if (player.EnumerateGrapplingHooks().Any(tuple => GetHooked(tuple.projectile) && tuple.projectile != proj)) {
			return false;
		}

		return true;
	}

	private static void PlayerGrappleMovementInjection(ILContext context)
	{
		var il = new ILCursor(context);
		bool debugAssembly = OverhaulMod.TMLAssembly.IsDebugAssembly();

		// Match 'GoingDownWithGrapple = true;' to prepare the code emitting location.
		il.GotoNext(
			MoveType.After,
			i => i.MatchStfld(typeof(Player), nameof(Player.GoingDownWithGrapple))
		);

		ILUtils.HijackIncomingLabels(il);

		int codeInsertionLocation = il.Index;

		// Match 'if (controlJump)' to mark a label.
		var skipVanillaCodeLabel = il.DefineLabel();

		if (!debugAssembly) {
			il.GotoNext(
				MoveType.Before,
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Player), nameof(Player.controlJump)),
				i => i.MatchBrfalse(out _)
			);
		} else {
			il.GotoNext(
				MoveType.Before,
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Player), nameof(Player.controlJump)),
				i => i.MatchStloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchBrfalse(out _)
			);
		}

		il.MarkLabel(skipVanillaCodeLabel);

		// Go back and emit actual code, now that we know that all matches succeeded.

		il.Index = codeInsertionLocation;

		il.Emit(OpCodes.Ldarg_0);
		il.EmitDelegate<Func<Player, bool>>(ShouldOverrideGrapplingHookPhysics);
		il.Emit(OpCodes.Brtrue, skipVanillaCodeLabel);
	}

	private static void PlayerUpdateInjection(ILContext context)
	{
		var il = new ILCursor(context);

		// Match 'WingAirLogicTweaks();' call as it's the nearest unique thing.
		il.GotoNext(
			MoveType.Before,
			i => i.MatchLdarg(0),
			i => i.MatchCall(typeof(Player), "WingAirLogicTweaks")
		);

		// Going back up, match a part of 'else if (grappling[0] == -1 && !tongued)'.
		var checkPredicates = new Func<Instruction, bool>[] {
			i => i.MatchLdarg(0),
			i => i.MatchLdfld(typeof(Player), nameof(Player.grappling)),
			i => i.MatchLdcI4(0),
			i => i.MatchLdelemI4(),
			i => i.MatchLdcI4(-1),
			i => i.MatchBneUn(out _)
		};

		il.GotoPrev(MoveType.Before, checkPredicates);

		// Emit code that skips over this check whenever we override physics.

		var skipThisCheckLabel = il.DefineLabel();

		ILUtils.HijackIncomingLabels(il);

		il.Emit(OpCodes.Ldarg_0);
		il.EmitDelegate<Func<Player, bool>>(ShouldOverrideGrapplingHookPhysics);
		il.Emit(OpCodes.Brtrue, skipThisCheckLabel);

		// Go forward and mark the label at the next right after.
		il.Index += checkPredicates.Length;

		il.MarkLabel(skipThisCheckLabel);
	}
}
