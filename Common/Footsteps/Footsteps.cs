// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Footsteps;

internal enum FootstepType
{
	Default,
	Jump,
	Land,
}

internal struct FootstepSounds : IComponent
{
	public SoundStyle? Step;
	public SoundStyle? Jump;
	public SoundStyle? Land;
}

internal struct FootstepCtx()
{
	public Entity? Entity = null;
	public bool AllowGoreInteraction = true;
	public bool AllowScreenShake = true;
	public bool AllowParticles = true;
	public bool ForceParticles = false;
	public float Volume = 1f;
	/// <summary> The origin used to alternate footstep positions when walking. </summary>
	public Vector2 Origin = new(0.5f, 1.0f);
	public required FootstepType Kind;
	public required Rectangle Hitbox;
	public required (Vector2 Cur, Vector2 Old) Velocity;
	public Point16? PointOverride;
	public FootstepSounds? SoundsOverride;
}

internal class FootstepSystem : ModSystem
{
	public static readonly ConfigEntry<bool> EnableMovementDust = new(ConfigSide.ClientOnly, true, "Visuals");

	public static Prefab DefaultFootstepSoundProvider { get; private set; }
	private static readonly ContentSet Dusty = nameof(Dusty);

	public override void OnModLoad()
	{
		DefaultFootstepSoundProvider = Prefabs.Get("StoneMaterial");
	}

	public static bool Footstep(in FootstepCtx ctx)
	{
		if (Main.dedServ) return false;

		Tile? tile = null;
		var kind = ctx.Kind;
		var originWorldPos = new Vector2(
			MathHelper.Lerp(ctx.Hitbox.Left, ctx.Hitbox.Right, ctx.Origin.X),
			MathHelper.Lerp(ctx.Hitbox.Top, ctx.Hitbox.Bottom, ctx.Origin.Y) - 4
		);
		var originTilePos = originWorldPos.ToTileCoordinates16();

		var stepTilePos = default(Point16);
		var stepWorldPos = default(Vector2);

		// Find a valid tile.
		if (ctx.PointOverride is { } p && p.IsInWorld() && Main.tile.TryGet(p, out Tile t) && t.HasTile && Main.tileSolid[t.TileType]) {
			tile = t;
			stepTilePos = p;
			stepWorldPos = p.ToWorldCoordinates(autoAddY: 0);
		} else {
			const float stepSize = 8;
			var xMax = (int)MathF.Ceiling(ctx.Hitbox.Width / stepSize) + 1;
			
			for (int y = 0; y < 2; y++) {
				for (int x = 0; x < xMax; x++) {
					var xStep = ((x + 1) / 2) * (x % 2 == 0 ? 1 : -1);
					var thisWorldPos = new Vector2(
						originWorldPos.X + (xStep * stepSize),
						originWorldPos.Y + (y * 16)
					);
					var thisTilePos = thisWorldPos.ToTileCoordinates16();
				
					if (Main.tile.TryGet(thisTilePos, out t) && t.HasUnactuatedTile) {
						tile = t;
						stepTilePos = thisTilePos;
						stepWorldPos = thisTilePos.ToWorldCoordinates(autoAddY: 0);
						break;
					}
				}
			}
		}

		Prefab? mainProvider = null;
		Prefab? extraProvider = null;
		float mainVolume = ctx.Volume;
		float extraVolume = ctx.Volume;

		// Try to get a footstep provider from the tile
		if (mainProvider == null && tile != null && PhysicalMaterials.TryGetTileMaterial(tile.Value.TileType, out var material)) {
			mainProvider = material;
		}

		// Check for nearby gore
		var collisionRect = ctx.Hitbox.Inflated(8, 8);
		for (int i = 0; i < Main.maxGore; i++) {
			if (Main.gore[i] is not OverhaulGore { active: true } gore) continue;
			if (!gore.sticky) continue;
			if (gore.MaterialPrefab is not { IsValid: true } mat) continue;
			if (!collisionRect.Intersects(gore.AABBRectangle)) continue;

			if (ctx.AllowGoreInteraction && kind is FootstepType.Jump or FootstepType.Land) {
				bool strong = extraProvider == null && kind is FootstepType.Land;
				var direction = (gore.Center.DirectionFrom(stepWorldPos) with { Y = -1f }).SafeNormalize(-Vector2.UnitY);
				gore.ApplyForce(direction * (strong ? 2.5f : 0.75f));
				gore.Damage(damageScale: strong ? 0.25f : 0.05f);
			}

			if (extraProvider == null) {
				extraProvider = mat;
				mainVolume *= 0.5f;
			}
		}

		// Particle effects.
		//TODO: Add water splashes?
		if (ctx.ForceParticles || (ctx.SoundsOverride == null && EnableMovementDust && ctx.AllowParticles)) {
			bool landing = ctx.Kind is FootstepType.Land && ctx.Velocity.Old.Y >= 3;
			bool jumpOrLand = ctx.Kind is FootstepType.Jump || landing;
			bool dirtyBlock = tile != null && Dusty.HasTile(tile.Value);
			bool fallingFast = MathF.Abs(ctx.Velocity.Old.Y) >= 10;
			bool xSpeedLow = MathF.Abs(ctx.Velocity.Cur.X) >= 3.5f;
			bool xSpeedMed = MathF.Abs(ctx.Velocity.Cur.X) >= 7.0f;
			bool xSpeedHigh = MathF.Abs(ctx.Velocity.Cur.X) >= 8.5f;
			if (ctx.ForceParticles || (((landing || xSpeedMed || (xSpeedLow && jumpOrLand)) && dirtyBlock) || xSpeedHigh)) {
				int goreType = fallingFast ? ModContent.GoreType<DustCloudMedium>() : ModContent.GoreType<DustCloudSmall>();
				var xVel = MathUtils.Clamp(ctx.Velocity.Cur.X * 0.05f, -1.5f, +1.5f) + Main.rand.NextFloat(-0.1f, 0.1f);
				var goreVel = new Vector2(xVel, 0);
				var gorePos = stepWorldPos + new Vector2(0, 2);
			
				if (Gore.NewGorePerfect(null, gorePos, goreVel, goreType) is { active: true } gore) {
					Main.instance.LoadGore(goreType);
					gore.position -= gore.AABBRectangle.Size() * new Vector2(0.5f, 1.0f);
				}
			}
		}

		//TODO: Implement leaves footsteps when those are added.

		void PlaySound(float volume, FootstepSounds? footstepSounds)
		{
			if (footstepSounds is not { } sounds) return;

			var sound = kind switch {
				FootstepType.Jump => sounds.Jump ?? sounds.Step,
				FootstepType.Land => sounds.Land ?? sounds.Step,
				_ => sounds.Step,
			};

			if (sound.HasValue) {
				SoundEngine.PlaySound(sound.Value with { Volume = sound.Value.Volume * volume }, stepWorldPos);
			}
		}

		if (ctx.Volume > 0f) {
			PlaySound(mainVolume, ctx.SoundsOverride ?? (mainProvider?.Get<FootstepSounds>()));
			PlaySound(extraVolume, extraProvider?.Get<FootstepSounds>());
		}

		if (kind == FootstepType.Land) {
			const float minSpeed = 5.5f;
			const float maxSpeed = 25.0f;
			bool isAnyPlayer = ctx.Entity is Player;
			bool isLocalPlayer = ctx.Entity == Main.LocalPlayer;
			float maxPower = isLocalPlayer ? 1.0f : 0.6f;
			var power01 = MathF.Pow(MathUtils.Clamp01((ctx.Velocity.Old.Y - minSpeed) / (maxSpeed - minSpeed)), 2.5f);
			var powerScaled = power01 * maxPower;
			var length = MathHelper.Lerp(0.80f, 1.60f, power01);
			// Main.NewText($"Power01: {power01:0.00}, powerScaled: {powerScaled:0.00}, length: {length:0.00}");

			if (ctx.AllowScreenShake) {
				var shake = new ScreenShake(powerScaled, length);
				ScreenShakeSystem.New(shake, !isLocalPlayer ? stepWorldPos : null);
			}

			if (isAnyPlayer && power01 > 0.10 && ctx.Volume > 0f) {
				SoundEngine.PlaySound(new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Footsteps/FallBig", 5) {
					Volume = power01 * 0.45f,
					PitchVariance = 0.2f,
					Pitch = MathHelper.Lerp(0.3f, 0.1f, power01),
				}, stepWorldPos);
			}
		}

		return true;
	}
}
