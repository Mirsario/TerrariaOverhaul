// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Debugging;

namespace TerrariaOverhaul.Common.GrapplingHooks;

public struct GrapplingHookStats
{
	private static readonly Dictionary<int, GrapplingHookStats> stats = [];
	private static readonly HashSet<int> grapplingTypesWarnedAbout = [];

	public float Range = 300f;
	public float PullSpeed = 12.5f;
	public float PullVelocity = 0.1f;
	public float RaiseSpeed = 5.0f;
	public float RaiseVelocity = 0.975f;
	public float LowerSpeed = 5.0f;
	public float LowerVelocity = 1.0f;

	public GrapplingHookStats() { }

	public static GrapplingHookStats Get(Player player, Projectile projectile)
	{
		if (projectile.type <= 0) {
			throw new IndexOutOfRangeException($"{projectile.type} <= 0");
		}

		if (stats.TryGetValue(projectile.type, out var result)) {
			return result;
		}

		if (projectile.type < ProjectileID.Count || projectile.ModProjectile is not ModProjectile modProjectile) {
			if (grapplingTypesWarnedAbout.Add(projectile.type)) {
				DebugSystem.Logger.Warn($"Vanilla grappling hook '{ContentSamples.ProjectilesByType[projectile.type].Name}' (ID: {projectile.type}) does not have stats assigned.");
			}

			return new();
		}

		result = new() {
			Range = modProjectile.GrappleRange(),
		};

		ProjectileLoader.GrapplePullSpeed(projectile, player, ref result.PullSpeed);

		return result;
	}

	public static void Set(int projectileType, GrapplingHookStats value)
		=> stats[projectileType] = value;

	static GrapplingHookStats()
	{
		// PHM Singlehooks
		Set(ProjectileID.Hook, new() {
			Range = 300f,
		});
		Set(ProjectileID.SquirrelHook, new() {
			Range = 300f,
		});
		Set(ProjectileID.GemHookAmethyst, new() {
			Range = 300f,
		});
		Set(ProjectileID.GemHookTopaz, new() {
			Range = 330f,
		});
		Set(ProjectileID.GemHookSapphire, new() {
			Range = 360f,
		});
		Set(ProjectileID.GemHookEmerald, new() {
			Range = 390f,
		});
		Set(ProjectileID.GemHookRuby, new() {
			Range = 420f,
		});
		Set(ProjectileID.AmberHook, new() {
			Range = 420f,
		});
		Set(ProjectileID.GemHookDiamond, new() {
			Range = 466f,
		});
		// PHM Multihooks					
		Set(ProjectileID.Web, new() {
			Range = 375f,
		});
		Set(ProjectileID.SkeletronHand, new() {
			Range = 350f,
		});
		Set(ProjectileID.SlimeHook, new() {
			Range = 300f,
		});
		Set(ProjectileID.FishHook, new() {
			Range = 400f,
		});
		Set(ProjectileID.IvyWhip, new() {
			Range = 400f,
		});
		Set(ProjectileID.BatHook, new() {
			Range = 500f,
		});
		Set(ProjectileID.CandyCaneHook, new() {
			Range = 400f,
		});
		// HM Singlehooks					
		Set(ProjectileID.DualHookBlue, new() {
			Range = 440f,
		});
		Set(ProjectileID.DualHookRed, new() {
			Range = 440f,
		});
		Set(ProjectileID.QueenSlimeHook, new() {
			Range = 500f,
		});
		Set(ProjectileID.StaticHook, new() {
			Range = 600f,
		});
		// HM Multihooks					
		Set(ProjectileID.TendonHook, new() {
			Range = 480f,
		});
		Set(ProjectileID.ThornHook, new() {
			Range = 480f,
		});
		Set(ProjectileID.IlluminantHook, new() {
			Range = 480f,
		});
		Set(ProjectileID.WormHook, new() {
			Range = 480f,
		});
		Set(ProjectileID.AntiGravityHook, new() {
			Range = 500f,
		});
		Set(ProjectileID.WoodHook, new() {
			Range = 550f,
		});
		Set(ProjectileID.ChristmasHook, new() {
			Range = 550f,
		});
		Set(ProjectileID.LunarHookSolar, new() {
			Range = 550f,
		});
		Set(ProjectileID.LunarHookVortex, new() {
			Range = 550f,
		});
		Set(ProjectileID.LunarHookNebula, new() {
			Range = 550f,
		});
		Set(ProjectileID.LunarHookStardust, new() {
			Range = 550f,
		});
	}
}
