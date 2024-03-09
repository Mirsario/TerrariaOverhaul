using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.GrapplingHooks;

// Could use more comments.
public class ProjectileGrapplingHookPhysics : GlobalProjectile
{
	public static readonly ConfigEntry<bool> EnableGrapplingHookPhysics = new(ConfigSide.Both, "Movement", nameof(EnableGrapplingHookPhysics), () => true);

	public const int GrapplingHookAIStyle = ProjAIStyleID.Hook;

	private static HashSet<int>? grapplingTypesWarnedAbout;
	private static Dictionary<int, float>? vanillaHookRangesInPixels;

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

		// Vanilla's data for this is hardcoded and not accessible. These stats are from the wiki.
		vanillaHookRangesInPixels = new Dictionary<int, float> {
			// PHM Singlehooks
			{ ProjectileID.Hook,                300f }, // ID 13
			{ ProjectileID.SquirrelHook,        300f },	// ID 865
			{ ProjectileID.GemHookAmethyst,     300f }, // ID 230
			{ ProjectileID.GemHookTopaz,        330f },	// ID 231
			{ ProjectileID.GemHookSapphire,     360f },	// ID 232
			{ ProjectileID.GemHookEmerald,      390f },	// ID 233
			{ ProjectileID.GemHookRuby,         420f },	// ID 234
			{ ProjectileID.AmberHook,           420f },	// ID 753
			{ ProjectileID.GemHookDiamond,      466f },	// ID 235
			// PHM Multihooks					
			{ ProjectileID.Web,                 375f },	// ID 165
			{ ProjectileID.SkeletronHand,       350f },	// ID 256
			{ ProjectileID.SlimeHook,           300f },	// ID 396
			{ ProjectileID.FishHook,            400f },	// ID 372
			{ ProjectileID.IvyWhip,             400f },	// ID 32
			{ ProjectileID.BatHook,             500f },	// ID 315
			{ ProjectileID.CandyCaneHook,       400f },	// ID 331
			// HM Singlehooks					
			{ ProjectileID.DualHookBlue,        440f },	// ID 73
			{ ProjectileID.DualHookRed,         440f },	// ID 74
			{ ProjectileID.QueenSlimeHook,      500f },	// ID 935
			{ ProjectileID.StaticHook,          600f },	// ID 652
			// HM Multihooks					
			{ ProjectileID.TendonHook,          480f },	// ID 486
			{ ProjectileID.ThornHook,           480f }, // ID 487
			{ ProjectileID.IlluminantHook,      480f },	// ID 488
			{ ProjectileID.WormHook,            480f },	// ID 489
			{ ProjectileID.AntiGravityHook,     500f },	// ID 446
			{ ProjectileID.WoodHook,            550f },	// ID 322
			{ ProjectileID.ChristmasHook,       550f },	// ID 332
			{ ProjectileID.LunarHookSolar,      550f },	// ID 646
			{ ProjectileID.LunarHookVortex,     550f },	// ID 647
			{ ProjectileID.LunarHookNebula,     550f },	// ID 648
			{ ProjectileID.LunarHookStardust,   550f },	// ID 649
		};
	}

	public override void Unload()
	{
		if (vanillaHookRangesInPixels != null) {
			vanillaHookRangesInPixels.Clear();

			vanillaHookRangesInPixels = null;
		}
	}

	public void ProjectileGrappleMovement(Player player, Projectile proj)
	{
		if (vanillaHookRangesInPixels == null) {
			throw new InvalidOperationException($"'{nameof(ProjectileGrappleMovement)}' called before '{nameof(Load)}'.");
		}

		var playerCenter = player.Center;
		var projCenter = proj.Center;
		float hookRange;

		if (proj.ModProjectile != null) {
			hookRange = proj.ModProjectile.GrappleRange();
		} else if (!vanillaHookRangesInPixels.TryGetValue(proj.type, out hookRange)) {
			// Fallback, not intended to be ran.
			hookRange = 512f;

			if ((grapplingTypesWarnedAbout ??= new()).Add(proj.type)) {
				DebugSystem.Logger.Warn($"Vanilla grappling hook '{proj.Name}' (ID: {proj.type}) does not have a hook range assigned. Please report this.");
			}
		}

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
			=> MathHelper.Clamp(distance, 0f, hookRange - 1f);

		float dist = ClampDistance(Vector2.Distance(playerCenter, projCenter));
		bool down = player.controlDown && maxDist < hookRange;
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

	public static bool ShouldOverrideGrapplingHookPhysics([NotNullWhen(true)] Player? player, [NotNullWhen(true)] Projectile? proj)
	{
		if (!EnableGrapplingHookPhysics) {
			return false;
		}

		if (player?.active != true || proj?.active != true) {
			return false;
		}

		if (proj.aiStyle != GrapplingHookAIStyle || !GetHooked(proj) || OverhaulProjectileTags.NoGrapplingHookSwinging.Has(proj.type)) {
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

		il.HijackIncomingLabels();

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
		
		il.HijackIncomingLabels();

		il.Emit(OpCodes.Ldarg_0);
		il.EmitDelegate<Func<Player, bool>>(ShouldOverrideGrapplingHookPhysics);
		il.Emit(OpCodes.Brtrue, skipThisCheckLabel);

		// Go forward and mark the label at the next right after.
		il.Index += checkPredicates.Length;

		il.MarkLabel(skipThisCheckLabel);
	}
}
