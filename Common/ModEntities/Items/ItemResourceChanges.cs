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
	public class ItemResourceChanges : GlobalItem
	{
		public const int PickupLifeTime = 300;
		public const int PickupGrabDelay = 30;
		public const int PickupHealAmount = 3;
		public const int PickupManaAmount = 5;
		public const float DefaultPickupRange = 160f;

		public static readonly int[] LifeTypes = {
			ItemID.Heart,
			ItemID.CandyApple,
			ItemID.CandyCane
		};
		public static readonly int[] ManaTypes = {
			ItemID.Star,
			ItemID.SoulCake,
			ItemID.SugarPlum
		};

		private bool isHeart;

		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Item item, bool lateInstantiation)
			=> LifeTypes.Contains(item.type) || ManaTypes.Contains(item.type);

		public override void SetDefaults(Item item)
		{
			isHeart = LifeTypes.Contains(item.type);
		}

		public override GlobalItem Clone(Item item, Item itemClone) => base.Clone(item, itemClone);

		public override void PostUpdate(Item item)
		{
			int lifeTime = item.timeSinceItemSpawned / 5;

			if(lifeTime >= PickupLifeTime) {
				item.active = false;
				return;
			}

			if(lifeTime < PickupGrabDelay) {
				return;
			}

			float GetSquaredGrabRange(Player p)
			{
				float range = DefaultPickupRange * DefaultPickupRange;

				if(isHeart ? p.lifeMagnet : p.manaMagnet) {
					range *= 2f;
				}

				return range;
			}

			var center = item.Center;

			var resultTuple = Main.player
				.Where(p => p != null && p.active && !p.dead && IsNeededByPlayer(p))
				.Select<Player, (Player player, float sqrDistance, float sqrGrabRange)>(p => (p, Vector2.DistanceSquared(p.Center, center), GetSquaredGrabRange(p)))
				.Where(tuple => tuple.sqrDistance < tuple.sqrGrabRange)
				.OrderBy(tuple => tuple.sqrDistance)
				.FirstOrDefault();

			if(resultTuple != default) {
				item.velocity += (resultTuple.player.Center - center).SafeNormalize(default) * (resultTuple.sqrDistance / resultTuple.sqrDistance);
			}

			if(!Main.dedServ) {
				float lightIntensity = GetIntensity(item);

				Lighting.AddLight(item.Center, (isHeart ? Vector3.UnitX : Vector3.UnitZ) * lightIntensity);
			}
		}

		public override bool CanPickup(Item item, Player player)
		{
			int lifeTime = item.timeSinceItemSpawned / 5;

			if(lifeTime < PickupGrabDelay || !IsNeededByPlayer(player) || !player.getRect().Intersects(item.getRect())) {
				return false;
			}

			int bonus = item.stack;

			if(isHeart) {
				bonus *= PickupHealAmount;

				player.statLife = Math.Min(player.statLife + bonus, player.statLifeMax2);

				player.HealEffect(bonus);
			} else {
				bonus *= PickupManaAmount;

				player.statMana = Math.Min(player.statMana + bonus, player.statManaMax);

				player.ManaEffect(bonus);
			}

			if(!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.Item30, player.Center);
			}

			/*if(Main.netMode == NetmodeID.Server) {
				item.whoAmI = ItemUtils.FindItemId(item);

				MultiplayerSystem.SendPacket(new PlayerPickupBonusMessage(player, item, isHeart, isHeart ? player.statLife : player.statMana, bonus));
			}*/

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

		private float GetIntensity(Item item)
		{
			int lifeTime = item.timeSinceItemSpawned / 5;

			return 1f - (float)Math.Pow(MathHelper.Clamp(lifeTime / (float)PickupLifeTime, 0f, 1f), 3f);
		}

		private bool IsNeededByPlayer(Player p)
		{
			return isHeart
				? p.statLife < p.statLifeMax2
				: p.statMana < p.statManaMax;
		}
	}
}
