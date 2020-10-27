using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.GameContent;
using Terraria.GameContent.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Fires;
using TerrariaOverhaul.Content.SimpleEntities;
using TerrariaOverhaul.Core.Systems.SimpleEntities;
using TerrariaOverhaul.Utilities.Extensions;
using Terraria.DataStructures;
using TerrariaOverhaul.Core.Systems.Input;
using Microsoft.Xna.Framework.Input;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Systems.Gores
{
	[Autoload(Side = ModSide.Client)]
	public class OverhaulGore : Gore, ILoadable
	{
		public static readonly SoundStyle GoreGroundHitSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Gore/GoreGroundHit", 8, volume: 0.65f, pitchVariance: 0.2f);

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

		public Vector2 Center => position + size * 0.5f;

		//Load-time
		public void Load(Mod mod) { }
		public void Unload() { }
		//In-game
		public void Init()
		{
			Main.instance.LoadGore(type);

			var texture = TextureAssets.Gore[type].Value;

			originalSize = new Vector2(texture.Width, texture.Height / Frame.ColumnCount);
			size = originalSize * scale;
			minDimension = Math.Min(size.X, size.Y);
			health = 1f;
		}
		public void PostUpdate()
		{
			var center = Center;
			var point = center.ToTileCoordinates16();

			if(!Main.tile.TryGet(point, out var tile)) {
				active = false;

				return;
			}

			if(Main.tileSolid[tile.type] && tile.active()) {
				//MoveGoreUpwards(point);
			} else if(tile.liquid > 0) {
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
			if(Main.dedServ || time < 5 && Main.rand.Next(4) != 0) {
				return false;
			}

			velocity += new Vector2(hitDirection.X == 0f ? hitDirection.X : Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, -0.5f)) * powerScale;
			health -= Main.rand.NextFloat(0.4f, 1.1f) * powerScale;

			if(health <= 0f) {
				Destroy(true);
			} else {
				SoundEngine.PlaySound(GoreGroundHitSound, position);
			}

			return true;
		}
		public void Destroy(bool allowEffects, Vector2 hitDirection = default)
		{
			active = false;

			if(!allowEffects) {
				return;
			}

			int maxSizeDimension = (int)Math.Max(size.X, size.Y);

			//Split into small pieces
			
			if(bleedColor.HasValue && type != ModContent.GoreType<GenericGore>()) {
				int numGore = maxSizeDimension / 6;

				for(int i = 0; i < numGore; i++) {
					if(NewGorePerfect(position, velocity + (hitDirection * 0.5f - Vector2.UnitY + Main.rand.NextVector2Circular(1f, 1f)), ModContent.GoreType<GenericGore>()) is OverhaulGore goreExt) {
						goreExt.bleedColor = bleedColor;
					}
				}
			}

			//Blood
			SpawnBlood(maxSizeDimension / 3);

			//Hit sounds
			SoundEngine.PlaySound(GoreGroundHitSound, position);

			/*if(bleedColor.HasValue) {
				SoundInstance.Create<OggSoundInstance, OggSoundInfo>("Gore", position, MathHelper.Lerp(0.2f, 1f, Math.Min(1f, maxSizeDimension * 0.015625f)), Main.rand.Range(0.925f, 1.2f), maxAmount: 2);
			}*/
		}
		public void SpawnBlood(int amount)
		{
			if(stopBleeding || !bleedColor.HasValue) {
				return;
			}

			for(int i = 0; i < amount; i++) {
				SimpleEntity.Instantiate<BloodParticle>(p => {
					p.position = GetRandomPoint();
					p.velocity = Vector2.Transform(Vector2.UnitX * 3f, Matrix.CreateRotationZ(rotation + MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f))));
					p.color = bleedColor.Value;
				});
			}
		}

		private void OnLiquidCollision(Tile tile)
		{
			//Evaporate in lava
			if(tile.lava()) {
				SoundEngine.PlaySound(FireSystem.ExtinguishSound, position);
				
				for(int i = 0; i < 5; i++) {
					Dust.NewDustPerfect(GetRandomPoint(), DustID.Smoke, Main.rand.NextVector2(-1f, -1f, 1f, 0f), 128, Color.White);
				}

				active = false;

				return;
			}

			//Water physics
			var center = Center;
			float yPosRelative = center.Y - (float)(Math.Floor(center.Y / 16f) * 16f);

			if(yPosRelative > 16f - tile.liquid / 255f * 16f) {
				velocity.X = MathHelper.Lerp(velocity.X, 0f, 0.5f / 60f);
				velocity.Y = MathHelper.Lerp(velocity.Y, -3f, 5f / 60f);

				//Create ripples
				if(prevVelocity.Length() >= 2f && Main.GameUpdateCount % 5 == 0) {
					((WaterShaderData)Filters.Scene["WaterDistortion"].GetShader()).QueueRipple(center, minDimension / 32f);
				}

				stopBleeding = true;
			}
		}
		private void Bleeding()
		{
			if(!stopBleeding && bleedColor.HasValue && Main.GameUpdateCount % 5 == 0 && velocity.Length() >= 0.5f && prevVelocity.Length() >= 0.5f) {
				SpawnBlood(1);
			}
		}
		private void Burning()
		{
			if(!onFire) {
				return;
			}

			if(Main.GameUpdateCount % 5 == 0) {
				Dust.NewDustPerfect(GetRandomPoint(), DustID.Torch, Scale: 2f).noGravity = true;
			}

			if(Main.GameUpdateCount % 45 == 0) {
				HitGore(Vector2.Zero, silent: true);

				if(!active) {
					return;
				}
			}
		}
		private void Bouncing()
		{
			if(velocity.Y == 0f) {
				//Vertical bouncing
				if(Math.Abs(prevVelocity.Y) >= 1f) {
					SoundEngine.PlaySound(GoreGroundHitSound, position);

					velocity.Y = -prevVelocity.Y * 0.66f;
				}

				//Friction
				velocity.X *= 0.97f;

				if(velocity.X > -0.01f && velocity.X < 0.01f) {
					velocity.X = 0f;
				}
			}

			//Horizontal bouncing
			if(velocity.X == 0f && Math.Abs(prevVelocity.X) >= 1f) {
				SoundEngine.PlaySound(GoreGroundHitSound, position);

				velocity.X = -prevVelocity.X * 0.66f;
			}
		}
		private void MoveGoreUpwards(Point16 point)
		{
			int upY = point.Y - 2;

			if(upY >= 0 && Main.tile.TryGet(point.X, upY, out var upTile) && (!Main.tileSolid[upTile.type] || !upTile.active())) {
				position.Y -= 1;
				velocity.Y = 0.000001f;
			}
		}
	}
}
