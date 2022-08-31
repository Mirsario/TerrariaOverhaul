using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.EntitySources;
using TerrariaOverhaul.Common.Fires;
using TerrariaOverhaul.Common.PhysicalMaterials;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Content.SimpleEntities;
using TerrariaOverhaul.Core.PhysicalMaterials;
using TerrariaOverhaul.Core.SimpleEntities;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public class OverhaulGore : Gore, ILoadable, IPhysicalMaterialProvider
{
	private const int GoreSoundMinCooldown = 10;
	private const int GoreSoundMaxCooldown = 25;

	public static readonly SoundStyle GoreHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreHit", 3) {
		Volume = 0.4f,
		PitchVariance = 0.2f
	};
	public static readonly SoundStyle GoreBreakSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSplatter", 2) {
		Volume = 0.15f,
		PitchVariance = 0.2f,
	};
	public static readonly SoundStyle GoreGroundHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSmallSplatter", 2) {
		Volume = 0.4f,
		PitchVariance = 0.2f,
	};

	private static readonly Dictionary<SoundStyle, ulong> goreSoundCooldowns = new();

	public bool OnFire;
	//public bool NoBlood;
	//public bool NoFallSounds;
	//public bool WasInLiquid;
	public bool StopBleeding;
	public Vector2 Size;
	public Vector2 OriginalSize;
	public float MinDimension;
	public int Time;
	public float Health;
	public Vector2 PrevVelocity;
	public Vector2 PrevPosition;
	public Color? BleedColor;
	public SoundStyle? CustomBounceSound;

	public Vector2 Center => position + Size * 0.5f;

	public PhysicalMaterial? PhysicalMaterial {
		get {
			PhysicalMaterial? result = null;

			if (ModGore is IPhysicalMaterialProvider provider) {
				result ??= provider.PhysicalMaterial;
			}

			if (BleedColor.HasValue) {
				result ??= ModContent.GetInstance<GorePhysicalMaterial>();
			}

			return result;
		}
	}

	// Load-time

	public void Load(Mod mod)
	{
		
	}

	public void Unload()
	{
		goreSoundCooldowns?.Clear();
	}

	// In-game
	
	public void Init()
	{
		Main.instance.LoadGore(type);

		var texture = TextureAssets.Gore[type].Value;

		OriginalSize = new Vector2(texture.Width / Frame.ColumnCount, texture.Height / Frame.RowCount);
		Size = OriginalSize * scale;
		MinDimension = Math.Min(Size.X, Size.Y);
		Health = 1f;
	}

	public void PostUpdate()
	{
		var center = Center;
		var point = center.ToTileCoordinates16();

		if (!Main.tile.TryGet(point, out var tile)) {
			active = false;

			return;
		}

		if (Main.tileSolid[tile.TileType] && tile.HasTile) {
			// MoveGoreUpwards(point);
		} else if (tile.LiquidAmount > 0) {
			OnLiquidCollision(tile);
		}

		Bleeding();
		Burning();
		Bouncing();

		PrevVelocity = velocity;
		PrevPosition = position;

		Time++;
	}

	public void CopyFrom(Gore gore)
	{
		typeof(Gore)
			.GetProperty(nameof(ModGore), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
			.SetValue(this, gore.ModGore);

		type = gore.type;
		light = gore.light;
		Frame = gore.Frame;
		alpha = gore.alpha;
		scale = gore.scale;
		sticky = gore.sticky;
		active = gore.active;
		rotation = gore.rotation;
		velocity = gore.velocity;
		position = gore.position;
		timeLeft = gore.timeLeft;
		drawOffset = gore.drawOffset;
		behindTiles = gore.behindTiles;
		frameCounter = gore.frameCounter;
	}

	public Vector2 GetRandomPoint() => position + Main.rand.NextVector2Square(Size.X, Size.Y);

	public bool HitGore(Vector2 hitDirection, float powerScale = 1f, bool silent = false)
	{
		if (Main.dedServ || Time < 5 && !Main.rand.NextBool(4)) {
			return false;
		}

		velocity += new Vector2(hitDirection.X == 0f ? hitDirection.X : Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f)) * powerScale;
		Health -= Main.rand.NextFloat(0.4f, 1.1f) * powerScale;

		if (Health <= 0f) {
			Destroy(silent: silent);
		} else if (!silent && BleedColor.HasValue) {
			TryPlaySound(GoreHitSound, position);
		}

		return true;
	}

	public void Destroy(bool immediate = false, bool silent = false, Vector2 hitDirection = default)
	{
		active = false;

		if (immediate) {
			return;
		}

		int maxSizeDimension = (int)Math.Max(Size.X, Size.Y);

		// Split into small pieces

		if (BleedColor.HasValue && type != ModContent.GoreType<GenericGore>()) {
			IEntitySource entitySource = new EntitySource_GoreDeath(this);
			int numGore = maxSizeDimension / 6;

			for (int i = 0; i < numGore; i++) {
				if (NewGorePerfect(entitySource, position, velocity + (hitDirection * 0.5f - Vector2.UnitY + Main.rand.NextVector2Circular(1f, 1f)), ModContent.GoreType<GenericGore>()) is OverhaulGore goreExt) {
					goreExt.BleedColor = BleedColor;
					goreExt.OnFire = OnFire;
				}
			}
		}

		// Blood
		SpawnBlood(maxSizeDimension, 2f);

		// Hit sounds
		if (!silent && BleedColor.HasValue) {
			TryPlaySound(GoreBreakSound, position);
		}

		/*if(bleedColor.HasValue) {
			SoundInstance.Create<OggSoundInstance, OggSoundInfo>("Gore", position, MathHelper.Lerp(0.2f, 1f, Math.Min(1f, maxSizeDimension * 0.015625f)), Main.rand.Range(0.925f, 1.2f), maxAmount: 2);
		}*/
	}

	public void SpawnBlood(int amount, float sprayScale = 1f)
	{
		if (StopBleeding || !BleedColor.HasValue) {
			return;
		}

		float maxHorizontalSpeed = 30f * sprayScale;
		float maxVerticalSpeed = 100f * sprayScale;

		for (int i = 0; i < amount; i++) {
			SimpleEntity.Instantiate<BloodParticle>(p => {
				p.position = GetRandomPoint();
				p.velocity = velocity * 30f + new Vector2(
					Main.rand.NextFloat(-maxHorizontalSpeed, maxHorizontalSpeed),
					Main.rand.NextFloat(-maxVerticalSpeed, 0f)
				);
				p.color = BleedColor.Value;
			});
		}
	}

	private void OnLiquidCollision(Tile tile)
	{
		// Evaporate in lava
		if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava) {
			TryPlaySound(FireSystem.ExtinguishSound, position);

			for (int i = 0; i < 5; i++) {
				Dust.NewDustPerfect(GetRandomPoint(), DustID.Smoke, Main.rand.NextVector2(-1f, -1f, 1f, 0f), 128, Color.White);
			}

			active = false;

			return;
		}

		// Water physics
		var center = Center;
		float yPosRelative = center.Y - (float)(Math.Floor(center.Y / 16f) * 16f);

		if (yPosRelative > 16f - (tile.LiquidAmount / 255f * 16f)) {
			velocity.X = MathHelper.Lerp(velocity.X, 0f, 0.5f / 60f);
			velocity.Y = MathHelper.Lerp(velocity.Y, -3f, 5f / 60f);

			// Create ripples
			if (PrevVelocity.Length() >= 2f && Main.GameUpdateCount % 5 == 0) {
				((WaterShaderData)Filters.Scene["WaterDistortion"].GetShader()).QueueRipple(center, MinDimension / 32f);
			}

			StopBleeding = true;
		}
	}

	private void Bleeding()
	{
		if (!StopBleeding && BleedColor.HasValue && Main.GameUpdateCount % 5 == 0 && velocity.Length() >= 0.5f && PrevVelocity.Length() >= 0.5f) {
			SpawnBlood(1);
		}
	}

	private void Burning()
	{
		if (!OnFire) {
			return;
		}

		uint offsettedUpdateCount = (uint)(position.X + Main.GameUpdateCount);

		if (offsettedUpdateCount % 5 == 0) {
			Dust.NewDustPerfect(GetRandomPoint(), DustID.Torch, Scale: 2f).noGravity = true;
		}

		if (offsettedUpdateCount % 30 == 0 && Main.rand.NextBool(3)) {
			HitGore(Vector2.Zero, silent: true);

			if (!active) {
				return;
			}
		}
	}

	private void Bouncing()
	{
		var bounceSound = CustomBounceSound ?? (BleedColor.HasValue ? GoreGroundHitSound : null);

		if (velocity.Y == 0f) {
			// Vertical bouncing
			if (Math.Abs(PrevVelocity.Y) >= 1f) {
				TryPlaySound(bounceSound, position);

				velocity.Y = -PrevVelocity.Y * 0.66f;
			}

			// Friction
			velocity.X *= 0.97f;

			if (velocity.X > -0.01f && velocity.X < 0.01f) {
				velocity.X = 0f;
			}
		}

		// Horizontal bouncing
		if (velocity.X == 0f && Math.Abs(PrevVelocity.X) >= 1f) {
			TryPlaySound(bounceSound, position);

			velocity.X = -PrevVelocity.X * 0.66f;
		}
	}

	private void MoveGoreUpwards(Point16 point)
	{
		int upY = point.Y - 2;

		if (upY >= 0 && Main.tile.TryGet(point.X, upY, out var upTile) && (!Main.tileSolid[upTile.TileType] || !upTile.HasTile)) {
			position.Y -= 1;
			velocity.Y = 0.000001f;
		}
	}

	// Makeshift optimization to not spam PlaySound and not enumerate any lists to check if there's anything playing.
	private static bool TryPlaySound(in SoundStyle? style, Vector2 position)
	{
		if (style == null) {
			return false;
		}


		var realStyle = style.Value;
		ulong tick = Main.GameUpdateCount;

		if (!goreSoundCooldowns.TryGetValue(realStyle, out ulong cooldownEnd) || tick >= cooldownEnd) {
			goreSoundCooldowns[realStyle] = tick + (ulong)Main.rand.Next(GoreSoundMinCooldown, GoreSoundMaxCooldown);

			SoundEngine.PlaySound(in realStyle, position);

			return true;
		}

		return false;
	}
}
