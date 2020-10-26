using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Content.SimpleEntities;
using TerrariaOverhaul.Core.Systems.SimpleEntities;

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
		public int width;
		public int height;
		public int scaledWidth;
		public int scaledHeight;
		public int minScaledDimm;
		//public int time;
		//public float health;
		public Vector2 prevVelocity;
		public Vector2 prevPosition;
		//public GorePreset preset;

		//Load-time
		public void Load(Mod mod) { }
		public void Unload() { }
		//In-game
		public void Init()
		{
			Main.instance.LoadGore(type);

			var texture = TextureAssets.Gore[type].Value;

			width = texture.Width;
			height = texture.Height / Frame.ColumnCount;
			scaledWidth = (int)(width * scale);
			scaledHeight = (int)(height * scale);
			minScaledDimm = Math.Min(scaledWidth, scaledHeight);
		}
		public void PostUpdate()
		{
			var goreCenter = new Vector2(
				position.X + scaledWidth * 0.5f,
				position.Y + scaledHeight * 0.5f
			);

			//Bleeding
			if(Main.GameUpdateCount % 6 == 0) { // && velocity.Length() >= 0.5f && prevVelocity.Length() >= 0.5f) {
				SimpleEntity.Instantiate<BloodParticle>(p => {
					p.position = goreCenter + Main.rand.NextVector2Square(-4f, 4f);
					p.velocity = Vector2.Transform(Vector2.UnitX * 3f, Matrix.CreateRotationZ(rotation + MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f))));
					p.color = Color.DarkRed;
				});
			}

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
	}
}
