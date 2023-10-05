using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;
using BitOperations = System.Numerics.BitOperations;

namespace TerrariaOverhaul.Common.BloodAndGore;

// This code doesn't support adding different types of particles, but is extremely optimized.
// If there's a need for that customization then perhaps the mod needs an ECS at this point.
[Autoload(Side = ModSide.Client)]
public sealed class ParticleSystem : ModSystem
{
	[StructLayout(LayoutKind.Auto)]
	public struct ParticleData
	{
		public static readonly ParticleData Default = new();

		private static readonly Vector2 DefaultGravity = new(0.0f, 300.0f);

		public uint LifeTime = default;
		public float Rotation = default;
		public Vector2 Position = default;
		public Vector2 Velocity = default;
		public Vector2 VelocityScale = Vector2.One;
		public Vector2 Scale = Vector2.One;
		public Vector2 Gravity = DefaultGravity;
		public Color Color = Color.White;
		// I hate this.
		public Vector2 OldPosition1 = default; //public fixed float OldPositions[6];
		public Vector2 OldPosition2 = default;
		public Vector2 OldPosition3 = default;

		public ParticleData() { }
	}

	public static readonly SoundStyle BloodDripSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/BloodDrip", 14) {
		Volume = 0.3f,
		PitchVariance = 0.2f
	};

	private const int BitsPerMask = sizeof(ulong) * 8;

	private static uint maxParticles;

	public static uint MaxParticles {
		get => maxParticles;
		set {
			maxParticles = Math.Max(64, BitOperations.RoundUpToPowerOf2(value));

			particles = new ParticleData[maxParticles];
			presenceMask = new ulong[Math.Max(1, maxParticles / BitsPerMask)];
		}
	}

	private static ParticleData[] particles = Array.Empty<ParticleData>();
	private static ulong[] presenceMask = Array.Empty<ulong>();

	public override void Load()
	{
		MaxParticles = 1024;
	}

	public override void PostDrawTiles()
		=> DrawParticles();

	public override void PostUpdateDusts()
		=> UpdateParticles();

	public static void SpawnParticles(ReadOnlySpan<ParticleData> newParticles)
	{
		Span<Color> colorSpan = stackalloc Color[newParticles.Length];

		for (int i = 0; i < newParticles.Length; i++) {
			int index = AllocateIndex();

			ref var particle = ref particles[index];

			particle = newParticles[i];
			particle.OldPosition1 = particle.OldPosition2 = particle.OldPosition3 = particle.Position;

			colorSpan[i] = particle.Color;
		}

		BloodColorRecording.AddColors(colorSpan);
	}

	public static void ConfigureParticles(Span<ParticleData> particles, Vector2 position, Vector2 velocity, Color color)
	{
		for (int i = 0; i < particles.Length; i++) {
			ref var particle = ref particles[i];

			particle = new() {
				Position = position,
				Velocity = velocity * (Main.rand.NextVector2Circular(1.0f, 1.0f) + Vector2.One) * 0.5f
			};

			float intensity = Main.rand.Next(3) switch {
				2 => 0.7f,
				1 => 0.85f,
				_ => 1f,
			};

			color.R = (byte)(color.R * intensity);
			color.G = (byte)(color.G * intensity);
			color.B = (byte)(color.B * intensity);

			particle.Color = color;
		}
	}

	private static int AllocateIndex()
	{
		int index, maskIndex, bitIndex, baseIndex;

		// Search for a free index.
		for (maskIndex = 0, baseIndex = 0; maskIndex < presenceMask.Length; maskIndex++, baseIndex += BitsPerMask) {
			bitIndex = BitOperations.TrailingZeroCount(~presenceMask[maskIndex]);

			if (bitIndex != BitsPerMask) {
				index = (ushort)(baseIndex + bitIndex);
				presenceMask[maskIndex] |= 1ul << bitIndex;

				return index;
			}
		}

		// Overwrite a random already-taken index if there's no free ones.
		index = Main.rand.Next(particles.Length);

		return index;
	}

	private static void UpdateParticles()
	{
		float logicDeltaTime = TimeSystem.LogicDeltaTime;
		var screenCenter = CameraSystem.ScreenCenter;

		const int MaxLifeTime = 10 * 60;
		const float MaxParticleDistance = 3000f;
		const float MaxParticleDistanceSqr = MaxParticleDistance * MaxParticleDistance;

		for (int maskIndex = 0, baseIndex = 0; maskIndex < presenceMask.Length; maskIndex++, baseIndex += BitsPerMask) {
			ref ulong maskRef = ref presenceMask[maskIndex];
			ulong maskCopy = maskRef;

			while (maskCopy != 0) {
				int bitIndex = BitOperations.TrailingZeroCount(maskCopy);

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void RemoveBit(ref ulong mask)
				{
					mask &= ~(1ul << bitIndex);
				}

				RemoveBit(ref maskCopy);

				ref var particle = ref particles[baseIndex + bitIndex];
				var tilePosition = particle.Position.ToTileCoordinates();

				// Keep position history up to date, caveman style.
				particle.OldPosition3 = particle.OldPosition2;
				particle.OldPosition2 = particle.OldPosition1;
				particle.OldPosition1 = particle.Position;
				particle.Velocity += particle.Gravity * logicDeltaTime;

				if (Vector2.DistanceSquared(particle.Position, screenCenter) >= MaxParticleDistanceSqr || !Main.tile.TryGet(tilePosition.X, tilePosition.Y, out var tile)) {
					// Too far away or outside the world
					RemoveBit(ref maskRef);
					continue;
				}

				if (tile.HasTile && Main.tileSolid[tile.TileType]) {
					// On tile collision
					if (Main.rand.NextBool(50)) {
						SoundEngine.PlaySound(BloodDripSound, particle.Position);
					}
					DecalSystem.AddDecals(DecalStyle.Default, particle.Position + particle.Velocity.SafeNormalize(default) * Main.rand.NextFloat(5f), particle.Color);
					RemoveBit(ref maskRef);
					continue;
				}

				if (tile.LiquidAmount > 0) {
					// On liquid collision
					RemoveBit(ref maskRef);
					continue;
				}

				particle.Position += particle.Velocity * particle.VelocityScale * logicDeltaTime;

				if (particle.LifeTime++ == MaxLifeTime) {
					RemoveBit(ref maskRef);
				}
			}
		}
	}

	private static void DrawParticles()
	{
		var screenCenter = CameraSystem.ScreenCenter;
		float maxScreenDimension = Math.Max(Main.screenWidth, Main.screenHeight);
		float maxParticleDistanceSqr = maxScreenDimension * maxScreenDimension;
		var spriteBatch = Main.spriteBatch;

		spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

		for (int maskIndex = 0, baseIndex = 0; maskIndex < presenceMask.Length; maskIndex++, baseIndex += BitsPerMask) {
			ref ulong maskRef = ref presenceMask[maskIndex];
			ulong maskCopy = maskRef;

			while (maskCopy != 0) {
				int bitIndex = BitOperations.TrailingZeroCount(maskCopy);

				maskCopy &= ~(1ul << bitIndex);

				ref var particle = ref particles[baseIndex + bitIndex];

				if (Vector2.DistanceSquared(particle.Position, screenCenter) >= maxParticleDistanceSqr) {
					continue;
				}

				var tilePosition = particle.Position.ToTileCoordinates();
				var usedColor = Lighting.GetColor(tilePosition.X, tilePosition.Y, particle.Color);
				var lineStart = particle.Position;
				var lineEnd = particle.OldPosition3;

				var delta = lineEnd - lineStart;
				float length = MathF.Ceiling(delta.SafeLength(0f) / 2f) * 2f;
				var direction = delta.SafeNormalize(Vector2.UnitX);

				lineEnd = lineStart + direction * length;

				lineStart = Vector2Utils.Floor(lineStart / 2f) * 2f;
				lineEnd = Vector2Utils.Floor(lineEnd / 2f) * 2f;

				spriteBatch.DrawLine(lineStart - Main.screenPosition, lineEnd - Main.screenPosition, usedColor, 2);
			}
		}

		spriteBatch.End();
	}
}
