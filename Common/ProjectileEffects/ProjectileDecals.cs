// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.ProjectileEffects;

[Autoload(Side = ModSide.Client)]
public sealed class ProjectileDecals : GlobalProjectile
{
	public record class Data
	{
		public Color Color { get; init; } = Color.White;
		public Vector2Int? Size { get; init; }
		public bool IfChunkExists { get; init; }
		public DecalStyle DecalStyle { get; init; } = DecalStyle.Default;
		public Asset<Texture2D>? Texture { get; init; }
	}

	public static Data? IcePreset { get; private set; }
	public static Data? BulletPreset { get; private set; }
	public static Data? ExplosionPreset { get; private set; }
	public static Data? IncendiaryPreset { get; private set; }

	private Vector2Int maxSize;

	public Data? OnTick { get; set; }
	public Data? OnDestroy { get; set; }
	public Data? OnTileCollision { get; set; }

	public override bool InstancePerEntity => true;

	public override void Load()
	{
		BulletPreset = new() {
			Texture = Mod.Assets.Request<Texture2D>("Assets/Textures/Decals/BulletDecal"),
			Size = new Vector2Int(8, 8),
		};

		IcePreset = new() {
			Texture = Mod.Assets.Request<Texture2D>("Assets/Textures/Decals/IceDecal"),
			Color = Color.White.WithAlpha(64),
			Size = new Vector2Int(16, 16),
		};

		ExplosionPreset = new() {
			Texture = Mod.Assets.Request<Texture2D>("Assets/Textures/Decals/ExplosionDecal"),
			Color = Color.White.WithAlpha(48),
		};

		IncendiaryPreset = ExplosionPreset with {
			Color = Color.White.WithAlpha(32),
		};
	}

	private static readonly ContentSet Bullet = nameof(Bullet);
	private static readonly ContentSet Freezing = nameof(Freezing);
	private static readonly ContentSet Explosive = nameof(Explosive);
	private static readonly ContentSet Incendiary = nameof(Incendiary);

	public override void SetDefaults(Projectile projectile)
	{
		if (Bullet.Has(projectile)) OnTileCollision = BulletPreset;
		if (Freezing.Has(projectile)) OnTileCollision = IcePreset;
		if (Explosive.Has(projectile)) OnDestroy = ExplosionPreset;

		if (Incendiary.Has(projectile)) {
			OnTick = IncendiaryPreset! with { Color = Color.White.WithAlpha(3) };
			OnDestroy = IncendiaryPreset;
		}
	}

	public override bool PreAI(Projectile projectile)
	{
		UpdateMaxSize(projectile);
		TryCreateDecal(projectile, OnTick);

		return true;
	}

	public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
	{
		UpdateMaxSize(projectile);
		TryCreateDecal(projectile, OnTileCollision, oldVelocity);

		return true;
	}

	public override void OnKill(Projectile projectile, int timeLeft)
	{
		UpdateMaxSize(projectile);
		TryCreateDecal(projectile, OnDestroy);
	}

	public void CreateDecal(Vector2 position, Data data)
	{
		if (data.Texture is not { IsLoaded: true, Value: Texture2D texture }) {
			texture = TextureAssets.MagicPixel.Value;
		}

		var color = data.Color;

		if (color.A != 255) {
			float multiplier = color.A / (float)byte.MaxValue;

			color.R = (byte)(color.R * multiplier);
			color.G = (byte)(color.G * multiplier);
			color.B = (byte)(color.B * multiplier);
		}

		DecalSystem.AddDecals(data.DecalStyle, new DecalInfo {
			Texture = texture,
			Position = position,
			Size = data.Size is Vector2Int size ? size : maxSize,
			Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
			Color = color,
			IfChunkExists = data.IfChunkExists,
		});
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void TryCreateDecal(Projectile projectile, Data? data, Vector2 offset = default)
	{
		if (data != null) {
			CreateDecal(projectile.Center + offset, data);
		}
	}

	private void UpdateMaxSize(Projectile projectile)
	{
		maxSize = Vector2Int.Max(maxSize, new Vector2Int(projectile.width, projectile.height));
	}
}
