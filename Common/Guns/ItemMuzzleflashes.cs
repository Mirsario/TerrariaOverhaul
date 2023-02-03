using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.TextureColors;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Guns;

// Too many array shenanigans over here.

[Autoload(Side = ModSide.Client)]
public sealed class ItemMuzzleflashes : ItemComponent
{
	public struct MuzzleflashStyle
	{
		private readonly SpriteFrame[] segmentFrames = new SpriteFrame[SegmentCount];

		public Asset<Texture2D> Texture;

		public Span<SpriteFrame> SegmentFrames => segmentFrames;

		public MuzzleflashStyle(Asset<Texture2D> texture, ReadOnlySpan<SpriteFrame> segmentFrames)
		{
			Texture = texture;

			segmentFrames.CopyTo(SegmentFrames);
		}
	}

	private const int SegmentCount = 3;

	private static Color[] defaultColors = null!;
	private static MuzzleflashStyle[] defaultStyles = null!;

	public Vector3 LightColor = Color.Gold.ToVector3();
	public uint DefaultMuzzleflashLength = 6;
	public int CurrentStyleIndex;

	private Timer timer;
	private uint timerMaxValue;
	private Color[] colors = defaultColors;
	private MuzzleflashStyle[] styles = defaultStyles;

	public uint LastLength { get; private set; }

	public bool IsVisible => timer.Active;
	public uint TimeLeft => timer.Value;
	public uint TimeInitial => timerMaxValue;
	public float AnimationProgress => 1f - (TimeLeft / MathF.Max(TimeInitial, 1f));

	public MuzzleflashStyle CurrentStyle => Styles[CurrentStyleIndex];

	public ReadOnlySpan<Color> Colors => colors;
	public ReadOnlySpan<MuzzleflashStyle> Styles => styles;

	public override void Load()
	{
		base.Load();

		var defaultTexture = Mod.Assets.Request<Texture2D>("Common/Guns/Muzzleflash");
		var defaultSpriteFrame = new SpriteFrame(3, 4) {
			PaddingX = 0,
			PaddingY = 0,
		};
		var framesSpan = (Span<SpriteFrame>)stackalloc SpriteFrame[SegmentCount];

		defaultStyles = new MuzzleflashStyle[3];
		defaultColors = new Color[SegmentCount] {
			ColorUtils.FromHexRgba(0xf78437_ff),
			ColorUtils.FromHexRgba(0xf5d881_ff),
			ColorUtils.FromHexRgba(0xffffff_ff),
		};

		for (int i = 0; i < defaultStyles.Length; i++) {
			for (int j = 0; j < SegmentCount; j++) {
				framesSpan[j] = defaultSpriteFrame.With((byte)i, (byte)j);
			}

			defaultStyles[i] = new MuzzleflashStyle(defaultTexture, framesSpan);
		}
	}

	public override void SetDefaults(Item item)
	{
		static bool CheckItem(Item item)
		{
			// Ignore swung and consumed items.
			if (item.useStyle != ItemUseStyleID.Shoot) {
				return false;
			}

			// Ignore silent weapons.
			if (item.UseSound is not SoundStyle useSound) {
				return false;
			}

			// Only apply to items that fire projectiles.
			if (item.shoot <= ProjectileID.None
			|| !ContentSamples.ProjectilesByType.TryGetValue(item.shoot, out var shotProjectile)) {
				return false;
			}

			/*
			// Ignore items that shoot projectiles that don't glow.
			if (shotProjectile.light <= 0f) {
				return false;
			}
			*/

			// Ignore wrong item types
			if (item.useAmmo > 0
			&& item.useAmmo < ItemLoader.ItemCount
			&& (
				!AmmoID.Sets.IsBullet[item.useAmmo]
				&& !AmmoID.Sets.IsRocket[item.useAmmo]
				&& item.useAmmo != AmmoID.Gel
				&& item.useAmmo != AmmoID.Flare
				&& item.useAmmo != AmmoID.Dart
			)) {
				return false;
			}

			// Ignore all mana consuming weapons, except laser guns.
			if (item.mana > 0) {
				if (!useSound.IsTheSameAs(SoundID.Item12)
				&& !useSound.IsTheSameAs(SoundID.Item157)
				&& !useSound.IsTheSameAs(SoundID.Item158)) {
					return false;
				}
			}

			// Ignore summons and anything that gives buffs.
			if (item.buffType > 0) {
				return false;
			}

			// Ignore channeled items
			if (item.channel) {
				return false;
			}

			return true;
		}

		if (!Enabled && CheckItem(ContentSamples.ItemsByType.TryGetValue(item.type, out var baseItem) ? baseItem : item)) {
			SetEnabled(item, true);
		}
	}

	public override void OnEnabled(Item item)
	{

	}

	public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		if (!ContentSamples.ProjectilesByType.TryGetValue(type, out var projectile)) {
			return true;
		}

		OverrideColorsFromProjectile(projectile);

		return true;
	}

	public override bool? UseItem(Item item, Player player)
	{
		if (Enabled) {
			ResetColors();
			StartMuzzleflash(null);
		}

		return base.UseItem(item, player);
	}

	public void StartMuzzleflash(uint? lengthOverride = null)
	{
		timerMaxValue = lengthOverride ?? DefaultMuzzleflashLength;

		timer.Set(lengthOverride ?? 12);

		CurrentStyleIndex = MathUtils.Modulo(CurrentStyleIndex + (Main.rand.NextBool() ? 1 : -1), Styles.Length);
	}

	public void SetStyles(ReadOnlySpan<MuzzleflashStyle> values)
	{
		values.CopyTo(styles != defaultStyles && styles.Length == values.Length ? styles : (styles = new MuzzleflashStyle[values.Length]));
	}

	public void SetColors(ReadOnlySpan<Color> values)
	{
		if (values.Length != SegmentCount) {
			throw new ArgumentException($"{nameof(ItemMuzzleflashes)}.{nameof(Colors)} may only be set to spans with size of '{SegmentCount}'.");
		}

		values.CopyTo(colors != defaultColors ? colors : (colors = new Color[SegmentCount]));
	}

	public void ResetColors()
		=> SetColors(defaultColors);

	public void ResetStyles()
		=> SetStyles(defaultStyles);

	private void OverrideColorsFromProjectile(Projectile projectile)
	{
		Main.instance.LoadProjectile(projectile.type);

		if (TextureAssets.Projectile[projectile.type] is not { IsLoaded: true } textureAsset) {
			return;
		}

		var averageColor = TextureColorSystem.GetAverageColor(textureAsset);
		Span<Color> newColors = stackalloc Color[SegmentCount];

		if (projectile.light > 0f) {
			for (int i = 0; i < SegmentCount; i++) {
				newColors[i] = Color.Lerp(averageColor, Color.White, i / (float)(SegmentCount - 1));
			}
		}

		SetColors(newColors);

		LightColor = newColors[0].ToVector3();
	}
}
