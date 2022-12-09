using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ProjectileEffects;

[Autoload(Side = ModSide.Client)]
public sealed class ProjectileDecals : GlobalProjectile
{
	public record class Data
	{
		public Color Color { get; init; } = Color.White;
		public Vector2Int? Size { get; init; }
		public bool IsClearing { get; init; }
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

	public override void SetDefaults(Projectile projectile)
	{
		if (OverhaulProjectileTags.Bullet.Has(projectile.type)) {
			OnTileCollision = BulletPreset;
		}

		if (OverhaulProjectileTags.Ice.Has(projectile.type)) {
			OnTileCollision = IcePreset;
		}

		if (OverhaulProjectileTags.Explosive.Has(projectile.type)) {
			OnDestroy = ExplosionPreset;
		}

		if (OverhaulProjectileTags.Incendiary.Has(projectile.type)) {
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

	public override void Kill(Projectile projectile, int timeLeft)
	{
		UpdateMaxSize(projectile);
		TryCreateDecal(projectile, OnDestroy);
	}

	public void CreateDecal(Vector2 position, Data data)
	{
		if (data.Texture is not { IsLoaded: true, Value: Texture2D texture }) {
			texture = TextureAssets.MagicPixel.Value;
		}

		var rect = new Rectangle((int)position.X, (int)position.Y, 0, 0);

		if (data.Size is Vector2Int size) {
			rect.Inflate(size.X / 2, size.Y / 2);
		} else {
			rect.Inflate(maxSize.X / 2, maxSize.Y / 2);
		}

		var color = data.Color;

		if (color.A != 255) {
			float multiplier = color.A / (float)byte.MaxValue;

			color.R = (byte)(color.R * multiplier);
			color.G = (byte)(color.G * multiplier);
			color.B = (byte)(color.B * multiplier);
		}

		if (data.IsClearing) {
			DecalSystem.ClearDecals(texture, rect, color);
		} else {
			DecalSystem.AddDecals(texture, rect, color);
		}
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
