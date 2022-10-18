using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ResourceDrops;

// Ugh, inheritance.
// Aim to do things here rather than in derivatives, using them mostly for data setup.
public abstract class ResourcePickupChanges<TThis> : GlobalItem
{
	public static int MaxLifeTime { get; set; }
	public static int? ForcedItemType { get; set; }
	public static Vector3 LightColor { get; set; } = Vector3.One;
	public static SoundStyle? PickupSound { get; set; }
	public static Asset<Texture2D>? TextureOverride { get; set; }
	public static int GrabDelay { get; set; } = 20;

	public override bool InstancePerEntity => false;

	protected override bool CloneNewInstances => true;

	public abstract void ApplyPickupEffect(Item item, Player player);

	public abstract float GetPickupRange(Item item, Player player);

	public abstract bool IsNeededByPlayer(Item item, Player player);

	public abstract void SpawnDust(Item item, int amount, Rectangle rectangle, Func<Vector2> velocityGetter);

	public abstract override bool AppliesToEntity(Item item, bool lateInstantiation);

	public override GlobalItem Clone(Item item, Item itemClone)
	{
		// All data is value types.

		return base.Clone(item, itemClone);
	}

	public override void PostUpdate(Item item)
	{
		if (ForcedItemType.HasValue) {
			item.type = ForcedItemType.Value;
		}

		int lifeTime = item.timeSinceItemSpawned / 5;

		if (lifeTime >= MaxLifeTime) {
			item.active = false;
			item.TurnToAir();
			return;
		}

		if (lifeTime < GrabDelay) {
			return;
		}

		var center = item.Center;

		// Visual effects
		if (!Main.dedServ) {
			float fadeOutFactor = GetFadeOutFactor(item);
			float flashFactor = MathF.Min(1f, GetFlashFactor() + 0.5f);

			// Light
			float lightFadeOutFactor = 1f - MathF.Pow(fadeOutFactor, 3f);
			float lightFlashFactor = MathHelper.Lerp(0.5f, 1f, flashFactor);
			var lightColor = LightColor;

			if (lightColor != Vector3.One) {
				Lighting.AddLight(item.Center, lightColor * lightFadeOutFactor * lightFlashFactor);
			}

			// Dusts
			if (Main.GameUpdateCount % (5 + (int)(fadeOutFactor * 60)) == 0) {
				SpawnDust(item, 1, item.GetRectangle(), () => item.velocity + new Vector2(Main.rand.NextFloat(-1f, 1f), -1f));
			}
		}

		// Accelerate towards players
		var resultTuple = ActiveEntities.Players
			.Where(p => !p.dead && IsNeededByPlayer(item, p))
			.Select<Player, (Player player, float sqrDistance, float grabRange)>(p => (p, Vector2.DistanceSquared(p.Center, center), GetPickupRange(item, p)))
			.Where(tuple => tuple.sqrDistance < tuple.grabRange * tuple.grabRange)
			.OrderBy(tuple => tuple.sqrDistance)
			.FirstOrDefault();

		if (resultTuple != default) {
			item.velocity += (resultTuple.player.Center - center).SafeNormalize(default) * (resultTuple.sqrDistance / resultTuple.sqrDistance);
		}
	}

	public override bool CanPickup(Item item, Player player)
	{
		int lifeTime = item.timeSinceItemSpawned / 5;

		if (lifeTime < GrabDelay || !IsNeededByPlayer(item, player) || !player.getRect().Intersects(item.getRect())) {
			return false;
		}

		ApplyPickupEffect(item, player);

		if (!Main.dedServ) {
			// Spawn dusts
			SpawnDust(item, 15, player.GetRectangle(), () => (player.velocity + item.velocity.RotatedByRandom(MathHelper.PiOver4)) * 0.5f);

			// Play sound if it's not null
			SoundEngine.PlaySound(PickupSound, !player.IsLocal() ? player.Center : null);
		}

		item.active = false;
		item.TurnToAir();

		return false;
	}

	public override bool PreDrawInWorld(Item item, SpriteBatch sb, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
	{
		scale = 1f;
		//scale *= GetFadeOutFactor(item);

		if (TextureOverride is { IsLoaded: true, Value: Texture2D texture }) {
			var sourceA = new Rectangle(0, 0, texture.Width, texture.Height / 2);
			var sourceB = sourceA with { Y = sourceA.Height };

			var position = item.Center - Main.screenPosition;
			var usedOrigin = sourceA.Center();

			var baseColor = Color.White;

			float fadeOutFactor = 1f - GetFadeOutFactor(item);
			byte fadeOutByte = (byte)(fadeOutFactor * byte.MaxValue);

			float flashFactor = GetFlashFactor();
			byte flashByte = (byte)(flashFactor * byte.MaxValue);

			var colorA = baseColor.MultiplyRGBA(new Color(fadeOutByte, fadeOutByte, fadeOutByte, fadeOutByte));
			var colorB = colorA.MultiplyRGBA(new Color(flashByte, flashByte, flashByte, flashByte));

			sb.Draw(texture, position, sourceA, colorA, rotation, usedOrigin, scale, SpriteEffects.None, 0f);
			sb.Draw(texture, position, sourceB, colorB, rotation, usedOrigin, scale, SpriteEffects.None, 0f);

			return false;
		}

		return true;
	}

	protected float GetLifeProgress(Item item)
	{
		int lifeTime = item.timeSinceItemSpawned / 5;

		return MathHelper.Clamp(lifeTime / (float)MaxLifeTime, 0f, 1f);
	}

	protected float GetFadeOutFactor(Item item)
	{
		float lifeProgress = GetLifeProgress(item);

		return (float)Math.Pow(lifeProgress, 3f);
	}

	protected float GetFlashFactor()
	{
		return MathF.Pow(MathF.Sin(TimeSystem.RenderTime * 4f) * 0.5f + 0.5f, 2f);
	}
}
