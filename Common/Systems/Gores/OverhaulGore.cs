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

namespace TerrariaOverhaul.Common.Systems.Gores
{
	[Autoload(Side = ModSide.Client)]
	public class OverhaulGore : Gore, ILoadable
	{
		public static readonly SoundStyle GoreGroundHitSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Gore/GoreGroundHit", 8, volume: 0.65f, pitchVariance: 0.2f);

		//public bool onFire;
		//public bool noBlood;
		//public bool noFallSounds;
		//public bool wasInLiquid;
		public bool stopBleeding;
		public Vector2 size;
		public Vector2 originalSize;
		public float minDimension;
		//public int time;
		//public float health;
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
				MoveGoreUpwards(point);
			} else if(tile.liquid > 0) {
				OnLiquidCollision(tile);
			}

			Bleeding();
			Bouncing();

			prevVelocity = velocity;
			prevPosition = position;
		}
		public void CopyFrom(Gore gore)
		{
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
			if(stopBleeding || !bleedColor.HasValue || Main.GameUpdateCount % 5 != 0 || velocity.Length() < 0.5f || prevVelocity.Length() < 0.5f) {
				return;
			}

			SimpleEntity.Instantiate<BloodParticle>(p => {
				p.position = GetRandomPoint();
				p.velocity = Vector2.Transform(Vector2.UnitX * 3f, Matrix.CreateRotationZ(rotation + MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f))));
				p.color = bleedColor.Value;
			});
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
