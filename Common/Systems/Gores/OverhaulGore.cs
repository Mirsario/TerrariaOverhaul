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
using TerrariaOverhaul.Common.PhysicalMaterials;
using TerrariaOverhaul.Common.Systems.Fires;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Content.SimpleEntities;
using TerrariaOverhaul.Core.Systems.PhysicalMaterials;
using TerrariaOverhaul.Core.Systems.SimpleEntities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.Systems.Gores
{
	[Autoload(Side = ModSide.Client)]
	public class OverhaulGore : Gore, ILoadable, IPhysicalMaterialProvider
	{
		private const int GoreSoundMinCooldown = 10;
		private const int GoreSoundMaxCooldown = 25;

		public static readonly ISoundStyle GoreHitSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreHit", 3, volume: 0.4f, pitchVariance: 0.2f);
		public static readonly ISoundStyle GoreBreakSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSplatter", 2, volume: 0.15f, pitchVariance: 0.2f);
		public static readonly ISoundStyle GoreGroundHitSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSmallSplatter", 2, volume: 0.4f, pitchVariance: 0.2f);

		private static Dictionary<ISoundStyle, ulong> goreSoundCooldowns;

		public bool onFire;
		//public bool noBlood;
		//public bool noFallSounds;
		//public bool wasInLiquid;
		public bool stopBleeding;
		public Vector2 size;
		public Vector2 originalSize;
		public float minDimension;
		public int time;
		public float health;
		public Vector2 prevVelocity;
		public Vector2 prevPosition;
		public Color? bleedColor;
		public ISoundStyle customBounceSound;

		public Vector2 Center => position + size * 0.5f;

		public PhysicalMaterial PhysicalMaterial {
			get {
				PhysicalMaterial result = null;

				if (ModGore is IPhysicalMaterialProvider provider) {
					result ??= provider.PhysicalMaterial;
				}

				if (bleedColor.HasValue) {
					result ??= ModContent.GetInstance<GorePhysicalMaterial>();
				}

				return result;
			}
		}

		//Load-time
		public void Load(Mod mod)
		{
			goreSoundCooldowns = new Dictionary<ISoundStyle, ulong>();
		}

		public void Unload()
		{
			if (goreSoundCooldowns != null) {
				goreSoundCooldowns.Clear();

				goreSoundCooldowns = null;
			}
		}

		//In-game
		public void Init()
		{
			Main.instance.LoadGore(type);

			var texture = TextureAssets.Gore[type].Value;

			originalSize = new Vector2(texture.Width / Frame.ColumnCount, texture.Height / Frame.RowCount);
			size = originalSize * scale;
			minDimension = Math.Min(size.X, size.Y);
			health = 1f;
		}

		public void PostUpdate()
		{
			var center = Center;
			var point = center.ToTileCoordinates16();

			if (!Main.tile.TryGet(point, out var tile)) {
				active = false;

				return;
			}

			if (Main.tileSolid[tile.type] && tile.IsActive) {
				//MoveGoreUpwards(point);
			} else if (tile.LiquidAmount > 0) {
				OnLiquidCollision(tile);
			}

			Bleeding();
			Burning();
			Bouncing();

			prevVelocity = velocity;
			prevPosition = position;

			time++;
		}

		public void CopyFrom(Gore gore)
		{
			typeof(Gore)
				.GetProperty(nameof(ModGore), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
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

		public Vector2 GetRandomPoint() => position + Main.rand.NextVector2Square(size.X, size.Y);

		public bool HitGore(Vector2 hitDirection, float powerScale = 1f, bool silent = false)
		{
			if (Main.dedServ || time < 5 && Main.rand.Next(4) != 0) {
				return false;
			}

			velocity += new Vector2(hitDirection.X == 0f ? hitDirection.X : Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f)) * powerScale;
			health -= Main.rand.NextFloat(0.4f, 1.1f) * powerScale;

			if (health <= 0f) {
				Destroy(silent: silent);
			} else if (!silent && bleedColor.HasValue) {
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

			int maxSizeDimension = (int)Math.Max(size.X, size.Y);

			//Split into small pieces

			if (bleedColor.HasValue && type != ModContent.GoreType<GenericGore>()) {
				IEntitySource entitySource = new EntitySource_GoreDeath(this);
				int numGore = maxSizeDimension / 6;

				for (int i = 0; i < numGore; i++) {
					if (NewGorePerfect(entitySource, position, velocity + (hitDirection * 0.5f - Vector2.UnitY + Main.rand.NextVector2Circular(1f, 1f)), ModContent.GoreType<GenericGore>()) is OverhaulGore goreExt) {
						goreExt.bleedColor = bleedColor;
						goreExt.onFire = onFire;
					}
				}
			}

			//Blood
			SpawnBlood(maxSizeDimension, 2f);

			//Hit sounds
			if (!silent && bleedColor.HasValue) {
				TryPlaySound(GoreBreakSound, position);
			}

			/*if(bleedColor.HasValue) {
				SoundInstance.Create<OggSoundInstance, OggSoundInfo>("Gore", position, MathHelper.Lerp(0.2f, 1f, Math.Min(1f, maxSizeDimension * 0.015625f)), Main.rand.Range(0.925f, 1.2f), maxAmount: 2);
			}*/
		}

		public void SpawnBlood(int amount, float sprayScale = 1f)
		{
			if (stopBleeding || !bleedColor.HasValue) {
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
					p.color = bleedColor.Value;
				});
			}
		}

		private void OnLiquidCollision(Tile tile)
		{
			//Evaporate in lava
			if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Lava) {
				IEntitySource entitySource = new EntitySource_GoreLiquidCollision(this);

				TryPlaySound(FireSystem.ExtinguishSound, position);

				for (int i = 0; i < 5; i++) {
					Dust.NewDustPerfect(entitySource, GetRandomPoint(), DustID.Smoke, Main.rand.NextVector2(-1f, -1f, 1f, 0f), 128, Color.White);
				}

				active = false;

				return;
			}

			//Water physics
			var center = Center;
			float yPosRelative = center.Y - (float)(Math.Floor(center.Y / 16f) * 16f);

			if (yPosRelative > 16f - (tile.LiquidAmount / 255f * 16f)) {
				velocity.X = MathHelper.Lerp(velocity.X, 0f, 0.5f / 60f);
				velocity.Y = MathHelper.Lerp(velocity.Y, -3f, 5f / 60f);

				//Create ripples
				if (prevVelocity.Length() >= 2f && Main.GameUpdateCount % 5 == 0) {
					((WaterShaderData)Filters.Scene["WaterDistortion"].GetShader()).QueueRipple(center, minDimension / 32f);
				}

				stopBleeding = true;
			}
		}

		private void Bleeding()
		{
			if (!stopBleeding && bleedColor.HasValue && Main.GameUpdateCount % 5 == 0 && velocity.Length() >= 0.5f && prevVelocity.Length() >= 0.5f) {
				SpawnBlood(1);
			}
		}

		private void Burning()
		{
			if (!onFire) {
				return;
			}

			uint offsettedUpdateCount = (uint)(position.X + Main.GameUpdateCount);
			
			if (offsettedUpdateCount % 5 == 0) {
				Dust.NewDustPerfect(new EntitySource_GoreUpdate(this), GetRandomPoint(), DustID.Torch, Scale: 2f).noGravity = true;
			}

			if (offsettedUpdateCount % 30 == 0 && Main.rand.Next(3) == 0) {
				HitGore(Vector2.Zero, silent: true);

				if (!active) {
					return;
				}
			}
		}

		private void Bouncing()
		{
			var bounceSound = customBounceSound ?? (bleedColor.HasValue ? GoreGroundHitSound : null);

			if (velocity.Y == 0f) {
				//Vertical bouncing
				if (Math.Abs(prevVelocity.Y) >= 1f) {
					if (bounceSound != null) {
						TryPlaySound(bounceSound, position);
					}

					velocity.Y = -prevVelocity.Y * 0.66f;
				}

				//Friction
				velocity.X *= 0.97f;

				if (velocity.X > -0.01f && velocity.X < 0.01f) {
					velocity.X = 0f;
				}
			}

			//Horizontal bouncing
			if (velocity.X == 0f && Math.Abs(prevVelocity.X) >= 1f) {
				if (bounceSound != null) {
					TryPlaySound(bounceSound, position);
				}

				velocity.X = -prevVelocity.X * 0.66f;
			}
		}

		private void MoveGoreUpwards(Point16 point)
		{
			int upY = point.Y - 2;

			if (upY >= 0 && Main.tile.TryGet(point.X, upY, out var upTile) && (!Main.tileSolid[upTile.type] || !upTile.IsActive)) {
				position.Y -= 1;
				velocity.Y = 0.000001f;
			}
		}

		//Makeshift optimization to not spam PlaySound and not enumerate any lists to check if there's anything playing.
		private static bool TryPlaySound(ISoundStyle style, Vector2 position)
		{
			ulong tick = Main.GameUpdateCount;

			if (!goreSoundCooldowns.TryGetValue(style, out ulong cooldownEnd) || tick >= cooldownEnd) {
				goreSoundCooldowns[style] = tick + (ulong)Main.rand.Next(GoreSoundMinCooldown, GoreSoundMaxCooldown);

				SoundEngine.PlaySound(style, position);

				return true;
			}

			return false;
		}
	}
}
