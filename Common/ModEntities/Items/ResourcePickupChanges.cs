using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items
{
	public abstract class ResourcePickupChanges : GlobalItem
	{
		public abstract int MaxLifetime { get; }

		public virtual int GrabDelay => 20;

		public override bool InstancePerEntity => true;

		public abstract void OnPickupReal(Item item, Player player);
		public abstract bool IsNeededByPlayer(Item item, Player player);
		public abstract override bool AppliesToEntity(Item item, bool lateInstantiation);

		public virtual float GetPickupRange(Item item, Player player) => 160f;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return base.Clone(item, itemClone);
		}
		public override void PostUpdate(Item item)
		{
			int lifeTime = item.timeSinceItemSpawned / 5;

			if(lifeTime >= MaxLifetime) {
				item.active = false;
				return;
			}

			if(lifeTime < GrabDelay) {
				return;
			}

			var center = item.Center;

			var resultTuple = Main.player
				.Where(p => p != null && p.active && !p.dead && IsNeededByPlayer(item, p))
				.Select<Player, (Player player, float sqrDistance, float grabRange)>(p => (p, Vector2.DistanceSquared(p.Center, center), GetPickupRange(item, p)))
				.Where(tuple => tuple.sqrDistance < tuple.grabRange * tuple.grabRange)
				.OrderBy(tuple => tuple.sqrDistance)
				.FirstOrDefault();

			if(resultTuple != default) {
				item.velocity += (resultTuple.player.Center - center).SafeNormalize(default) * (resultTuple.sqrDistance / resultTuple.sqrDistance);
			}
		}
		public override bool CanPickup(Item item, Player player)
		{
			int lifeTime = item.timeSinceItemSpawned / 5;

			if(lifeTime < GrabDelay || !IsNeededByPlayer(item, player) || !player.getRect().Intersects(item.getRect())) {
				return false;
			}

			OnPickupReal(item, player);

			if(!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.Item30, player.Center);
			}

			item.active = false;
			item.TurnToAir();

			return false;
		}
		public override Color? GetAlpha(Item item, Color lightColor)
		{
			float progress = GetIntensity(item);
			byte alpha = (byte)(255 * progress);

			return new Color(255, 255, 255, alpha);
		}
		public override bool PreDrawInWorld(Item item, SpriteBatch sb, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			float intensity = GetIntensity(item);

			scale *= MathHelper.Lerp(0f, 0.75f, intensity);

			return true;
		}

		protected float GetIntensity(Item item)
		{
			int lifeTime = item.timeSinceItemSpawned / 5;

			return 1f - (float)Math.Pow(MathHelper.Clamp(lifeTime / (float)MaxLifetime, 0f, 1f), 3f);
		}
	}
}
