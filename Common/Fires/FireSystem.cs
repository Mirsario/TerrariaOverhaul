// Copyright (c) 2020-2025 Mirsario, Rartrin, & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Content.Flames;
using TerrariaOverhaul.Core.Chunks;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.Fires;

public enum FlameKind : byte
{
	Default,
	Cursed,
	Demon,
	Frost,
	Ichor,
	Ultrabright,
}

public struct FlameType()
{
	public required Asset<Texture2D> Texture;
	public int AssociatedTile = -1;
	public Vector3 LightColor;
	public Color BackgroundColor;
}

public struct TileFlame()
{
	public FlameKind FlameKind;
	public byte LengthInSeconds = 1;
	public byte Strength;
	public byte Extinguish;
	public byte SpreadsLeft;

	public readonly bool OnFire => Strength != 0;
	public readonly float StrengthFactor => Strength / (float)byte.MaxValue;
	public readonly float TicksPerSecond => byte.MaxValue / (float)LengthInSeconds;
}

public struct ChunkFlames() : IComponent
{
	public uint NumFlames { get; private set; }
	public TileFlame[,] Flames = new TileFlame[Chunks.MaxChunkSize, Chunks.MaxChunkSize];

	public void ModifyFlameCount(int delta) {
		NumFlames = checked((uint)(NumFlames + delta));
	}
}

public sealed class FireSystem : ModSystem
{
	private ref struct FlameCtx
	{
		public bool IsFlammable;
		public byte IgnitionPerTick;
		public byte IdlePerTick;
		public byte ExtinguishPerTick;
		public Tile Tile;
		public Vector2Int LocalPoint;
		public Vector2Int GlobalPoint;
		public TileFlame FlameCopy;
		public FlamesCache FlamesCache;
		public ref TileFlame Flame;
		public ref readonly FlameType FlameType;
	}

	public static readonly SoundStyle ExtinguishSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Fire/Extinguish") {
		PitchVariance = 0.1f
	};

	private static readonly List<Vector2Int> activeChunks = new();
	private static readonly Point16[][] selfFrame8WayLookup = null!;
	private static readonly Asset<Texture2D>[] flameTextureLookup = null!;
	private static readonly FlameType[] FlameTypes = new FlameType[Enum.GetValues<FlameKind>().Length];
	private static readonly TileFlame[,] dummyFlames = new TileFlame[Chunks.MaxChunkSize, Chunks.MaxChunkSize];

	private static readonly ContentSet Flammable = nameof(Flammable);
	private static readonly ContentSet AlwaysCharred = nameof(AlwaysCharred);
	private static readonly ContentSet NeverCharred = nameof(NeverCharred);
	private static readonly ContentSet NonIgnitable = nameof(NonIgnitable);
	private static readonly ContentSet Grass = nameof(Grass);

	private static uint UpdateFrequency => 4;

	static FireSystem()
	{
		[UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "selfFrame8WayLookup")]
		static extern ref Point16[][] GetSelfFrame8WayLookup(Framing? framing);

		if (!Main.dedServ) {
			selfFrame8WayLookup = GetSelfFrame8WayLookup(null);
		}
	}

	public override void SetStaticDefaults()
	{
		FlameTypes[(int)FlameKind.Default] = new FlameType {
			Texture = TextureAssets.Tile[TileID.LivingFire],
			AssociatedTile = TileID.LivingFire,
			LightColor = new(1.0f, 0.3f, 0.1f),
			BackgroundColor = new(255, 135, 135, 255),
		};
		FlameTypes[(int)FlameKind.Cursed] = new FlameType {
			Texture = TextureAssets.Tile[TileID.LivingCursedFire],
			AssociatedTile = TileID.LivingCursedFire,
			LightColor = new(0.1f, 1.0f, 0.1f),
			BackgroundColor = new(160, 192, 160, 160),
		};
		FlameTypes[(int)FlameKind.Demon] = new FlameType {
			Texture = TextureAssets.Tile[TileID.LivingDemonFire],
			AssociatedTile = TileID.LivingDemonFire,
			LightColor = new(0.5f, 0.5f, 1.0f),
			BackgroundColor = new(192, 192, 192, 255),
		};
		FlameTypes[(int)FlameKind.Frost] = new FlameType {
			Texture = TextureAssets.Tile[TileID.LivingFrostFire],
			AssociatedTile = TileID.LivingFrostFire,
			LightColor = new(0.0f, 0.5f, 1.0f),
			BackgroundColor = new(192, 192, 192, 255),
		};
		FlameTypes[(int)FlameKind.Ichor] = new FlameType {
			Texture = TextureAssets.Tile[TileID.LivingIchor],
			AssociatedTile = TileID.LivingIchor,
			LightColor = new(1.0f, 1.0f, 0.0f),
			BackgroundColor = new(255, 135, 135, 255),
		};
		FlameTypes[(int)FlameKind.Ultrabright] = new FlameType {
			Texture = TextureAssets.Tile[TileID.LivingUltrabrightFire],
			AssociatedTile = TileID.LivingUltrabrightFire,
			LightColor = new(1.0f, 1.0f, 1.0f),
			BackgroundColor = new(192, 192, 192, 255),
		};
	}

	public unsafe override void PostUpdateEverything()
	{
		UpdateAllChunks();
	}

	public override void PostDrawTiles()
	{
		DrawAllChunks();
	}

	public static void SetFlame(Vector2Int point, TileFlame flame)
	{
		if (!WorldGen.InWorld(point.X, point.Y)) return;
		if (!Chunks.TryGetOrCreateChunkAtTilePosition(point, out var chunk)) return;

		var tile = Main.tile[point.X, point.Y];
		var chunkInfo = chunk.Entity.Get<ChunkInfo>();

		//if (!IsPointFlammable(point) && !IsPointFlammable(point with { Y = point.Y + 1 }))
		if (!tile.HasTile && tile.WallType == 0 && Framing.GetTileSafely(point.X, point.Y + 1) is { } other && !other.HasTile && other.WallType == 0)
			return;

		if (!chunk.Entity.Has<ChunkFlames>()) chunk.Entity.Add(new ChunkFlames());
		ref var flames = ref chunk.Entity.Get<ChunkFlames>();

		var positionInChunk = point - new Vector2Int(chunkInfo.TileRectangle.X, chunkInfo.TileRectangle.Y);
		ref var current = ref flames.Flames[positionInChunk.X, positionInChunk.Y];

		if (current.OnFire != flame.OnFire) {
			flames.ModifyFlameCount(flame.OnFire ? 1 : -1);
			current = flame;
		}

		if (!activeChunks.Contains(chunkInfo.Position))
			activeChunks.Add(chunkInfo.Position);

		current.Extinguish = flame.Extinguish;
	}

	private static bool IsPointIgnitable(Vector2Int point)
	{
		if (!WorldGen.InWorld(point.X, point.Y)) return false;
		var tile = Main.tile[point.X, point.Y];

		if (tile.HasTile && !NonIgnitable.HasTile(tile))
			return true;

		if (tile.WallType != 0 && !NonIgnitable.HasWall(tile))
			return true;

		return false;
	}
	private static bool IsPointFlammable(Vector2Int point)
	{
		if (!WorldGen.InWorld(point.X, point.Y)) return false;
		var tile = Main.tile[point.X, point.Y];

		if (tile.HasTile && (Flammable.HasTile(tile) || tile.TileType == ModContent.TileType<BurnedGrass>()))
			return true;

		if (tile.WallType != 0 && Flammable.HasWall(tile))
			return true;

		return false;
	}
	private static int PickBurnedTileVariant(Tile tile)
	{
		return ModContent.TileType<BurnedGrass>();
	}

	private static unsafe void UpdateAllChunks()
	{
		bool tick = TimeSystem.UpdateCount % UpdateFrequency == 0;

		for (int i = 0; i < activeChunks.Count; i++) {
			if (!Chunks.TryGetChunk(activeChunks[i], out var chunk) || !chunk.Entity.Has<ChunkFlames>()) {
				activeChunks.RemoveAt(i--);
				continue;
			}

			UpdateSingleChunk(chunk);
		}
	}
	private static unsafe void UpdateSingleChunk(Chunk chunk)
	{
		var chunkInfo = chunk.Entity.Get<ChunkInfo>();
		var chunkTopLeft = new Vector2Int(chunkInfo.TileRectangle.X, chunkInfo.TileRectangle.Y);

		const int Length = Chunks.MaxChunkSize * Chunks.MaxChunkSize;
		ref var flames = ref chunk.Entity.Get<ChunkFlames>();
		FlameCtx ctx = default;
		ctx.FlamesCache = new FlamesCache(chunk);

		fixed (TileFlame* ptr = flames.Flames) {
			ctx.LocalPoint.X = 0;
			ctx.LocalPoint.Y = 0;

			for (int j = 0; j < Length; j++) {
				ctx.Flame = ref ptr[j];
				ctx.FlameCopy = ctx.Flame;

				if (!ctx.Flame.OnFire)
					goto End;

				ctx.GlobalPoint = chunkTopLeft + ctx.LocalPoint;
				ctx.Tile = Framing.GetTileSafely(ctx.GlobalPoint.X, ctx.GlobalPoint.Y);
				ctx.FlameType = ref FlameTypes[(int)ctx.Flame.FlameKind];

				SetupFlameStats(ref ctx);
				ProgressFlame(ref ctx);
				ConvertTiles(ref ctx);
				SpreadFlames(ref ctx);
				FlameLighting(ref ctx);
				FlameDecals(ref ctx);

				if (ctx.Flame.Strength == 0) {
					flames.ModifyFlameCount(-1);
					ctx.Flame = new();
				}

				End:
				if (++ctx.LocalPoint.Y == Chunks.MaxChunkSize) {
					ctx.LocalPoint.Y = 0;
					ctx.LocalPoint.X += 1;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SetupFlameStats(ref FlameCtx ctx)
	{
		if (IsPointFlammable(ctx.GlobalPoint)) {
			ctx.IsFlammable = true;
			ctx.IgnitionPerTick = 2;
			ctx.IdlePerTick = 1;
			ctx.ExtinguishPerTick = 3;
		} else {
			ctx.IsFlammable = false;
			ctx.IgnitionPerTick = 1;
			ctx.IdlePerTick = 255;
			ctx.ExtinguishPerTick = 4;
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ProgressFlame(ref FlameCtx ctx)
	{
		if (ctx.Flame.Extinguish < byte.MaxValue) {
			if (ctx.Flame.Strength < byte.MaxValue) {
				ctx.Flame.Strength = (byte)Math.Min(ctx.Flame.Strength + ctx.IgnitionPerTick, byte.MaxValue);
			} else {
				ctx.Flame.Extinguish = (byte)Math.Min(ctx.Flame.Extinguish + ctx.IdlePerTick, byte.MaxValue);
			}
		} else {
			if ((ctx.Flame.Strength - ctx.ExtinguishPerTick) < 0) {
				ctx.Flame.Strength = 0;
			} else {
				ctx.Flame.Strength -= ctx.ExtinguishPerTick;
			}
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ConvertTiles(ref FlameCtx ctx)
	{
		if (ctx.Flame.Strength != ctx.FlameCopy.Strength && ctx.Flame.Strength == byte.MaxValue) {
			if (Grass.HasTile(ctx.Tile)) {
				int dstType = PickBurnedTileVariant(ctx.Tile);
				if (ctx.Tile.TileType != dstType) WorldGen.ConvertTile(ctx.GlobalPoint.X, ctx.GlobalPoint.Y, dstType);
			}
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void SpreadFlames(ref FlameCtx ctx)
	{
		if (ctx.Flame.SpreadsLeft == 0) return;
		if (ctx.Flame.Strength != byte.MaxValue) return;
		if (ctx.Flame.Strength == ctx.FlameCopy.Strength) return;

		ctx.Flame.SpreadsLeft = (byte)(ctx.Flame.SpreadsLeft - 1);

		for (int y = -1; y <= +1; y++) {
			for (int x = -1; x <= +1; x++) {
				if (x == 0 || Math.Abs(x) == Math.Abs(y)) continue;
				
				var chunkTilePoint = ctx.LocalPoint + new Vector2Int(x, y);
				var targetFlame = ctx.FlamesCache.Get(chunkTilePoint.X, chunkTilePoint.Y);
				if (targetFlame.OnFire) continue;

				var worldTilePoint = ctx.GlobalPoint + new Vector2Int(x, y);
				if (!IsPointFlammable(worldTilePoint)) continue;

				SetFlame(worldTilePoint, new TileFlame {
					Strength = 1,
					Extinguish = 0,
					FlameKind = ctx.Flame.FlameKind,
					SpreadsLeft = ctx.Flame.SpreadsLeft,
					LengthInSeconds = ctx.Flame.LengthInSeconds,
				});
			}
		}
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void FlameLighting(ref FlameCtx ctx)
	{
		if (Main.dedServ) return;
		if (ctx.Flame.Strength == 0) return;

		float str = ctx.Flame.StrengthFactor;
		Vector3 lightColor = ctx.FlameType.LightColor;
		Lighting.AddLight(ctx.GlobalPoint.X, ctx.GlobalPoint.Y, lightColor.X * str, lightColor.Y * str, lightColor.Z * str);
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void FlameDecals(ref FlameCtx ctx)
	{
		if (Main.dedServ) return;
		if (ctx.Flame.Strength == 0) return;

		if (NeverCharred.HasTile(ctx.Tile) || NeverCharred.HasWall(ctx.Tile)) return;
		if (!ctx.IsFlammable && !AlwaysCharred.HasTile(ctx.Tile) && !AlwaysCharred.HasWall(ctx.Tile)) return;

		int frame = (((ctx.GlobalPoint.X + ctx.GlobalPoint.Y) % 3) != 0) ? ((((ctx.GlobalPoint.X + ctx.GlobalPoint.Y) % 2) == 0) ? 2 : 1) : 0;
		var srcRect = new Rectangle(48 * frame, 0, 48, 48);
		var decalTexture = ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/Decals/Charring", AssetRequestMode.ImmediateLoad).Value;

		DecalSystem.AddDecals(DecalStyle.Default, new DecalInfo {
			Layers = DecalLayerFlags.All,
			Texture = decalTexture,
			Position = new Vector2((ctx.GlobalPoint.X + 0.5f) * WorldUtils.TileSizeInPixels, (ctx.GlobalPoint.Y + 0.5f) * WorldUtils.TileSizeInPixels),
			SrcRect = srcRect,
			Color = new Color(2, 2, 2, 2),
			Scale = Vector2.One * 1.00f,
		});
	}

	private static void DrawAllChunks()
	{
		// Request relevant textures.
		foreach (var flameType in FlameTypes) {
			if (flameType.AssociatedTile >= 0) {
				Main.instance.LoadTiles(flameType.AssociatedTile);
			}
		}

		foreach (var chunk in Chunks.IterateVisibleChunks()) {
			DrawChunkFlames(chunk);
		}
	}
	// Rendering algorithm courtesy of Rartrin.
	private unsafe static void DrawChunkFlames(Chunk chunk)
	{
		if (!chunk.Entity.Has<ChunkFlames>()) return;
		ref var data = ref chunk.Entity.Get<ChunkFlames>();

		if (data.NumFlames == 0) return;

		const int TileSize = WorldUtils.TileSizeInPixels;
		const int NumFrameSteps = 3;
		const int NumFlameFrames = 5;

		var sb = Main.spriteBatch;
		var chunkInfo = chunk.Entity.Get<ChunkInfo>();
		var flamesCache = new FlamesCache(chunk);

		sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

		for (int i = 0; i < 2; i++) {
			bool drawingSolids = i == 1;

			for (int y = 0; y < Chunks.MaxChunkSize; y++) {
				for (int x = 0; x < Chunks.MaxChunkSize; x++) {
					ref var flame = ref data.Flames[x, y];
					if (!flame.OnFire) continue;

					int xx = chunkInfo.TileRectangle.X + x;
					int yy = chunkInfo.TileRectangle.Y + y;
					var tile = Framing.GetTileSafely(xx, yy);
					bool isSolid = tile.HasTile && Main.tileSolid[tile.TileType];
					if (isSolid != drawingSolids) continue;

					ref readonly var flameType = ref FlameTypes[(int)flame.FlameKind];
					var light = drawingSolids ? Color.White : flameType.BackgroundColor;
					float drawX = (xx << 4) - Main.screenPosition.X;
					float drawY = (yy << 4) - Main.screenPosition.Y;
					// Up/Left/Right/Down
					Tile t;
					bool uHasFire = flamesCache.Get(x, y - 1).OnFire && (!drawingSolids || ((t = Framing.GetTileSafely(xx, yy)).HasTile && Main.tileSolid[t.TileType]));
					bool lHasFire = flamesCache.Get(x - 1, y).OnFire && (!drawingSolids || ((t = Framing.GetTileSafely(xx, yy)).HasTile && Main.tileSolid[t.TileType]));
					bool rHasFire = flamesCache.Get(x + 1, y).OnFire && (!drawingSolids || ((t = Framing.GetTileSafely(xx, yy)).HasTile && Main.tileSolid[t.TileType]));
					bool dHasFire = flamesCache.Get(x, y + 1).OnFire && (!drawingSolids || ((t = Framing.GetTileSafely(xx, yy)).HasTile && Main.tileSolid[t.TileType]));
					//(uHasFire, lHasFire, rHasFire, dHasFire) = (false, false, false, false);

					var frameBits = new BitsByte(uHasFire, lHasFire, rHasFire, dHasFire, uHasFire & lHasFire, uHasFire & rHasFire, dHasFire & lHasFire, dHasFire & rHasFire);
					var frame = selfFrame8WayLookup[frameBits][((x << 8) + y) % NumFrameSteps];
					var fireTexture = flameType.Texture.Value;
					int animFrameY = Main.tileFrame[TileID.LivingFire] * (18 * NumFlameFrames);

					if (flame.Strength >= byte.MaxValue) {
						sb.Draw(fireTexture, new Vector2(drawX, drawY), new Rectangle(frame.X, frame.Y + animFrameY, 16, 16), light);
						continue;
					}

					// Converts power to a [0..1] range, then multiplies it by the pixel range, 16.
					int length = (int)(TileSize * (flame.StrengthFactor));
					int offset = TileSize - length;
					var drawPos = new Vector2(drawX, drawY);
					var source = new Rectangle(frame.X, frame.Y + animFrameY, TileSize, TileSize);
					bool checkForSpecialCases = true;

					if (uHasFire != dHasFire) {
						if (uHasFire)
							source.Y += offset;
						else
							drawPos.Y += offset;

						source.Height = length;
						checkForSpecialCases = false;
					}

					if (rHasFire != lHasFire) {
						if (rHasFire)
							drawPos.X += offset;
						else
							source.X += offset;

						source.Width = length;
						checkForSpecialCases = false;
					}

					if (checkForSpecialCases) {
						if (uHasFire != rHasFire) {
							length /= 2;
							offset = TileSize - length;

							int lookup = ((x << 8) + y) % NumFrameSteps;

							if (uHasFire) {
								var frameU = selfFrame8WayLookup[1][lookup];
								var frameD = selfFrame8WayLookup[8][lookup];

								sb.Draw(fireTexture, new Vector2(drawX, drawY), new Rectangle(frameU.X, frameU.Y + offset + animFrameY, TileSize, length), light);
								sb.Draw(fireTexture, new Vector2(drawX, drawY + offset), new Rectangle(frameD.X, frameD.Y + animFrameY, TileSize, length), light);
							} else {
								var frameL = selfFrame8WayLookup[2][lookup];
								var frameR = selfFrame8WayLookup[4][lookup];

								sb.Draw(fireTexture, new Vector2(drawX, drawY), new Rectangle(frameL.X + offset, frameL.Y + animFrameY, length, TileSize), light);
								sb.Draw(fireTexture, new Vector2(drawX + offset, drawY), new Rectangle(frameR.X, frameR.Y + animFrameY, length, TileSize), light);
							}

							continue;
						}

						if (!uHasFire && !rHasFire) {
							drawPos += new Vector2(8);

							sb.Draw(fireTexture, drawPos, source, light, 0, new Vector2(8), length / (float)TileSize, SpriteEffects.None, 0);
							continue;
						}
					}

					sb.Draw(fireTexture, drawPos, source, light);
				}
			}
		}

		sb.End();
	}

	// Used to quickly access neighbouring chunks' flame arrays. 
	private readonly ref struct FlamesCache
	{
		public readonly TileFlame[,] X0Y0, X1Y0, X2Y0;
		public readonly TileFlame[,] X0Y1, X1Y1, X2Y1;
		public readonly TileFlame[,] X0Y2, X1Y2, X2Y2;

		public FlamesCache(Chunk chunk)
		{
			var chunkPos = chunk.Entity.Get<ChunkInfo>().Position;
			X0Y0 = GetArray(chunkPos + new Vector2Int(-1, -1));
			X1Y0 = GetArray(chunkPos + new Vector2Int(+0, -1));
			X2Y0 = GetArray(chunkPos + new Vector2Int(+1, -1));
			X0Y1 = GetArray(chunkPos + new Vector2Int(-1, +0));
			X1Y1 = chunk.Entity.Has<ChunkFlames>() ? chunk.Entity.Get<ChunkFlames>().Flames : dummyFlames;
			X2Y1 = GetArray(chunkPos + new Vector2Int(+1, +0));
			X0Y2 = GetArray(chunkPos + new Vector2Int(-1, +1));
			X1Y2 = GetArray(chunkPos + new Vector2Int(+0, +1));
			X2Y2 = GetArray(chunkPos + new Vector2Int(+1, +1));
		}

		//TODO: Could be much faster.
		public ref TileFlame Get(int x, int y)
		{
			int dirX = x < 0 ? -1 : x < Chunks.MaxChunkSize ? 0 : 1;
			int dirY = y < 0 ? -1 : y < Chunks.MaxChunkSize ? 0 : 1;
			int xx = MathUtils.Modulo(x, Chunks.MaxChunkSize);
			int yy = MathUtils.Modulo(y, Chunks.MaxChunkSize);

			switch (dirX, dirY) {
				case (-1, -1): return ref this.X0Y0[xx, yy];
				case (+0, -1): return ref this.X1Y0[xx, yy];
				case (+1, -1): return ref this.X2Y0[xx, yy];
				case (-1, +0): return ref this.X0Y1[xx, yy];
				case (+0, +0): return ref this.X1Y1[xx, yy];
				case (+1, +0): return ref this.X2Y1[xx, yy];
				case (-1, +1): return ref this.X0Y2[xx, yy];
				case (+0, +1): return ref this.X1Y2[xx, yy];
				case (+1, +1): return ref this.X2Y2[xx, yy];
				default: throw new IndexOutOfRangeException();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TileFlame[,] GetArray(Vector2Int position)
			=> Chunks.TryGetChunk(position, out var c) && c.Entity.Has<ChunkFlames>() ? c.Entity.Get<ChunkFlames>().Flames : dummyFlames;
	}
}
