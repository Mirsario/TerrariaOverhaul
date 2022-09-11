using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
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
	public static readonly ConfigEntry<bool> EnableGrapplingHookPhysics = new(ConfigSide.Both, "PlayerMovement", nameof(EnableGrapplingHookPhysics), () => true);

	public const int GrapplingHookAIStyle = 7;


	private static HashSet<int>? grapplingTypesWarnedAbout;
	private static Dictionary<int, float>? vanillaHookRangesInPixels;
	private static Func<Projectile, Tile, bool>? canTileBeLatchedOnFunc;

	private float maxDist;
	private bool noPulling;

	public override bool InstancePerEntity => true;

	public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation)
		=> lateInstantiation && projectile.aiStyle == GrapplingHookAIStyle;

	public override void Load()
	{
		canTileBeLatchedOnFunc = typeof(Projectile)
			.GetMethod("AI_007_GrapplingHooks_CanTileBeLatchedOnTo", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?
			.CreateDelegate<Func<Projectile, Tile, bool>>()
			?? throw new InvalidOperationException($"Unable to get delegate for '{nameof(ProjectileGrapplingHookPhysics)}.{nameof(canTileBeLatchedOnFunc)}'");

		On.Terraria.Player.GrappleMovement += (orig, player) => {
			if (ShouldOverrideGrapplingHookPhysics(player, out _)) {
				return;
			}

			orig(player);
		};
		On.Terraria.Player.JumpMovement += (orig, player) => {
			if (ShouldOverrideGrapplingHookPhysics(player, out _)) {
				PlayerJumpOffGrapplingHook(player);
			}

			orig(player);
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

	public override bool PreAI(Projectile projectile)
	{
		if (ShouldOverrideGrapplingHookPhysics(projectile, out var player)) {
			ProjectileGrappleMovement(player, projectile);

			return false;
		}

		return true;
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

		if (player.dead) {
			proj.Kill();
			SetHooked(proj, false);

			return;
		}

		var mountedCenter = player.MountedCenter;
		var mountedOffset = mountedCenter - projCenter;

		proj.rotation = (float)Math.Atan2(mountedOffset.Y, mountedOffset.X) - 1.57f;
		proj.velocity = Vector2.Zero;

		// Check if the tile that this is latched to has disappeared.

		if (!Main.tile.TryGet(projCenter.ToTileCoordinates16(), out var tile) || !CanTileBeLatchedOnto(proj, tile)) {
			SetHooked(proj, false);
			proj.Kill();

			return;
		}

		// Dismount if currently using a mount
		if (player.mount.Active) {
			player.mount.Dismount(player);
		}

		// Reset movement, but not jumps.
		player.RefreshMovementAbilities();

		player.rocketFrame = false;
		player.canRocket = false;
		player.rocketRelease = false;
		player.fallStart = (int)(playerCenter.Y / 16f);
		player.sandStorm = false;

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

	public static bool ShouldOverrideGrapplingHookPhysics(Player player, out Projectile? projectile)
	{
		projectile = player != null ? Main.projectile.FirstOrDefault(p => p != null && p.active && p.aiStyle == GrapplingHookAIStyle && p.owner == player.whoAmI && GetHooked(p)) : null;

		return ShouldOverrideGrapplingHookPhysics(player, projectile);
	}

	public static bool ShouldOverrideGrapplingHookPhysics(Projectile? proj, out Player player)
	{
		player = proj?.GetOwner()!;

		return ShouldOverrideGrapplingHookPhysics(player, proj);
	}

	public static bool ShouldOverrideGrapplingHookPhysics(Player? player, Projectile? proj)
	{
		if (!EnableGrapplingHookPhysics) {
			return false;
		}

		if (player?.active != true) {
			return false;
		}

		if (proj?.active != true || proj.aiStyle != GrapplingHookAIStyle || !GetHooked(proj) || OverhaulProjectileTags.NoGrapplingHookSwinging.Has(proj.type)) {
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

	private static bool CanTileBeLatchedOnto(Projectile projectile, Tile tile)
	{
		if (!tile.HasTile || tile.IsActuated) {
			return false;
		}

		if (tile.TileType != TileID.MinecartTrack && !canTileBeLatchedOnFunc!(projectile, tile)) {
			return false;
		}

		return true;
	}
}
