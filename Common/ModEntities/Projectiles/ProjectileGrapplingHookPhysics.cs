using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Projectiles
{
	// Could use more comments.
	public class ProjectileGrapplingHookPhysics : GlobalProjectile
	{
		public const int GrapplingHookAIStyle = 7;

		private static Dictionary<int, float> vanillaHookRangesInTiles;

		private float maxDist;
		private bool noPulling;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation)
			=> lateInstantiation && projectile.aiStyle == GrapplingHookAIStyle;

		public override void Load()
		{
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
			vanillaHookRangesInTiles = new Dictionary<int, float> {
				// PHM Singlehooks
				{ ProjectileID.Hook,             18.750f },
				{ ProjectileID.GemHookAmethyst,     18.750f },
				{ ProjectileID.SquirrelHook,        19.000f },
				{ ProjectileID.GemHookTopaz,        20.625f },
				{ ProjectileID.GemHookSapphire,     22.500f },
				{ ProjectileID.GemHookEmerald,      24.375f },
				{ ProjectileID.GemHookRuby,         26.250f },
				{ ProjectileID.AmberHook,           27.500f },
				{ ProjectileID.GemHookDiamond,      29.125f },
				// PHM Multihooks
				{ ProjectileID.Web,                  15.625f },
				{ ProjectileID.SkeletronHand,       21.875f },
				{ ProjectileID.SlimeHook,           18.750f },
				{ ProjectileID.FishHook,            25.000f },
				{ ProjectileID.IvyWhip,             28.000f },
				{ ProjectileID.BatHook,             31.250f },
				{ ProjectileID.CandyCaneHook,       25.000f },
				// HM Singlehooks
				{ ProjectileID.DualHookBlue,     27.500f },
				{ ProjectileID.DualHookRed,         27.500f },
				{ ProjectileID.QueenSlimeHook,      30.000f },
				{ ProjectileID.StaticHook,          37.500f },
				// HM Multihooks
				{ ProjectileID.ThornHook,            30.000f },
				{ ProjectileID.IlluminantHook,      30.000f },
				{ ProjectileID.WormHook,            30.000f },
				{ ProjectileID.TendonHook,          30.000f },
				{ ProjectileID.AntiGravityHook,     31.250f },
				{ ProjectileID.WoodHook,            34.375f },
				{ ProjectileID.ChristmasHook,       34.375f },
				{ ProjectileID.LunarHookNebula,     34.375f },
				{ ProjectileID.LunarHookSolar,      34.375f },
				{ ProjectileID.LunarHookStardust,   34.375f },
				{ ProjectileID.LunarHookVortex,     34.375f },
			};
		}

		public override void Unload()
		{
			if (vanillaHookRangesInTiles != null) {
				vanillaHookRangesInTiles.Clear();

				vanillaHookRangesInTiles = null;
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
			var playerCenter = player.Center;
			var projCenter = proj.Center;
			float hookRange = proj.ModProjectile?.GrappleRange() ?? (vanillaHookRangesInTiles.TryGetValue(proj.type, out float vanillaRangeInTiles) ? vanillaRangeInTiles * 16f : 512f);

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

			if (!Main.tile.TryGet(projCenter.ToTileCoordinates16(), out var tile) || !tile.IsActive || tile.IsActuated || (!Main.tileSolid[tile.type] && tile.type != TileID.MinecartTrack)) {
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

			float dist = Vector2.Distance(playerCenter, projCenter);
			bool down = player.controlDown && maxDist < hookRange;
			bool up = player.controlUp;

			if (pull || ((up || down) && up != down)) {
				dist += pull ? -12.5f : up ? -5f : 5f;
				dist = MathHelper.Clamp(dist, 10f, hookRange - 32f);
				maxDist = dist;
				player.velocity *= pull ? 0.1f : up ? 0.975f : 1f;
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
		// 
		public static bool GetHooked(Projectile proj) => proj.ai[0] == 2f;
		public static void SetHooked(Projectile proj, bool newValue) => proj.ai[0] = newValue ? 2f : 0f;
		// 
		public static bool ShouldOverrideGrapplingHookPhysics(Player player, out Projectile projectile)
		{
			projectile = player != null ? Main.projectile.FirstOrDefault(p => p != null && p.active && p.aiStyle == GrapplingHookAIStyle && p.owner == player.whoAmI && GetHooked(p)) : null;

			return ShouldOverrideGrapplingHookPhysics(player, projectile);
		}

		public static bool ShouldOverrideGrapplingHookPhysics(Projectile proj, out Player player)
		{
			player = proj?.GetOwner();

			return ShouldOverrideGrapplingHookPhysics(player, proj);
		}

		public static bool ShouldOverrideGrapplingHookPhysics(Player player, Projectile proj)
		{
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
	}
}
