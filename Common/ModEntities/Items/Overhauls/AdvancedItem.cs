using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Projectiles;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls
{
	public abstract partial class AdvancedItem : ItemOverhaul
	{
		/*protected static int shotId;
		protected static bool callCustomShoot;
		protected static AdvancedItem shootingItem;

		public int forcedUsesLeft;

		protected bool skipStartUse;
		protected bool skipStartUseChecks;
		protected bool allowConsume;

		public virtual int ManaUse => player.GetWeaponManaUsage(item);
		public virtual int ConsumePerUse => 1;
		public virtual int AmmoPerUse => 1;
		public virtual bool DoUseMana => true;
		public virtual bool CanShoot => true;
		public virtual bool AutoReuse => item.autoReuse;
		public virtual bool CanStartUse => true;
		public virtual bool PlayUseSound => true;

		public virtual bool PreCustomShoot(ref int shotsNum, ref int projectilesPerShot) => true;
		public virtual void CustomShoot(int shotId, int projId, ref int projectileType, ref Vector2 position, ref Vector2 velocity, ref int damage, ref float knockback, ref float ai0, ref float ai1) { }
		public virtual void PostCustomShoot(int shotsNum, int projectilesPerShot) { }
		public virtual void OnProjectileCreated(Projectile proj) { }
		public virtual void ModifyUseTime(ref int useAnimation, ref int useTime, ref int reuseDelay) { }

		public virtual void OnUseStart()
		{
			if(forcedUsesLeft == 0) {
				oPlayer.hadCritOnUseStart = oPlayer.critCharge >= 1f;
			}
		}
		public virtual void TryStartUse()
		{
			bool forceUse = forcedUsesLeft > 0;

			if(forceUse || oPlayer.controlPrimaryUse && oPlayer.noItemUseTimer == 0) {
				StartUse(false);

				if(forceUse) {
					forcedUsesLeft--;
				}
			}
		}
		public virtual void StartUse(bool skipItemTime)
		{
			int manaUse = ManaUse;

			if(manaUse > 0 && !oPlayer.UseMana(manaUse, DoUseMana)) {
				oPlayer.NoAmmo();
				return;
			}

			oPlayer.noAmmoNotified = false;

			if(!skipItemTime) {
				bool autoReuse = AutoReuse;

				oPlayer.ApplyItemTime(item);

				if(autoReuse && !itemDefault.autoReuse) {
					player.itemTime += 5;
					player.itemAnimation += 5;
				}

				ModifyUseTime(ref player.itemAnimation, ref player.itemTime, ref player.reuseDelay);

				if(!autoReuse) {
					oPlayer.needsRelease = true;
				}
			}

			if(!Main.dedServ && item.UseSound != null && PlayUseSound) {
				Main.PlaySound(item.UseSound, player.Center);
			}

			OnUseStart();

			AmmoInfo? ammo = null;

			if(item.useAmmo > 0) {
				int ammoPerUse = AmmoPerUse;
				var arr = oPlayer.GetAmmo(item, ammoPerUse > 0 ? ammoPerUse : 1, item.useAmmo);

				if(arr.Length < ammoPerUse) {
					return;
				}

				ammo = arr[0];

				if(ammoPerUse > 0) {
					oPlayer.TakeAmmo(item, ammoPerUse, item.useAmmo);
				}
			}

			Item itemToReset = null;

			if(item.consumable) {
				allowConsume = true;

				bool consumeItem = ItemLoader.ConsumeItem(item, player);

				allowConsume = false;

				if(consumeItem) {
					int consumePerUse = ConsumePerUse;

					var realItem = oPlayer.IsLocal && Main.mouseItem != null && !Main.mouseItem.IsAir ? player.inventory[58] : item;

					realItem.stack -= consumePerUse;

					if(realItem.stack <= 0) {
						itemToReset = realItem;
					}
				}
			}

			if(oPlayer.IsLocal && itemDefault.shoot > 0) {
				int shotsNum = 1;
				int projectilesPerShot = 1;

				var newProjectiles = ProjectileUtils.StartProjectileRecording();

				try {
					if(PreCustomShoot(ref shotsNum, ref projectilesPerShot)) {
						int prevShoot = item.shoot;

						int baseProjectile = GetDefaultShootProjectile(ammo);
						var basePosition = player.Center;
						var baseVelocity = oPlayer.LookDirection * (item.shootSpeed + (ammo?.shootSpeed ?? 0f));
						int baseDamage = player.GetWeaponDamage(item) + (ammo?.damage ?? 0);
						float baseKnockback = player.GetWeaponKnockback(item, item.knockBack + (ammo?.knockback ?? 0f));

						for(shotId = 0; shotId < shotsNum; shotId++) {
							//Copy variables
							int shotProjectile = baseProjectile;
							var shotPosition = basePosition;
							var shotVelocity = baseVelocity;
							int shotDamage = baseDamage;
							float shotKnockback = baseKnockback;

							shootingItem = this;
							callCustomShoot = true;

							try {
								bool result = true;

								try { result = PlayerHooks.Shoot(player, item, ref shotPosition, ref shotVelocity.X, ref shotVelocity.Y, ref shotProjectile, ref shotDamage, ref shotKnockback); }
								catch { }

								if(result) {
									try { result = ItemLoader.Shoot(item, player, ref shotPosition, ref shotVelocity.X, ref shotVelocity.Y, ref shotProjectile, ref shotDamage, ref shotKnockback); }
									catch { }

									if(result) {
										callCustomShoot = false;

										for(int projId = 0; projId < projectilesPerShot; projId++) {
											//Copy variables
											int projectile = shotProjectile;
											var position = shotPosition;
											var velocity = shotVelocity;
											int damage = shotDamage;
											float knockback = shotKnockback;
											float ai0 = 0f;
											float ai1 = 0f;

											CustomShoot(shotId, projId, ref projectile, ref position, ref velocity, ref damage, ref knockback, ref ai0, ref ai1);

											item.shoot = projectile;

											var proj = Projectile.NewProjectileDirect(position, velocity, projectile, damage, knockback, Main.myPlayer, ai0, ai1);

											proj.ai[0] = ai0;
											proj.ai[1] = ai1;
										}
									}
								}
							}
							catch { }

							shootingItem = null;
							callCustomShoot = false;
						}

						ReduceUsesLeft();

						item.shoot = prevShoot;

						PostCustomShoot(shotsNum, projectilesPerShot);
					}
				}
				catch { }

				ProjectileUtils.StopProjectileRecording(newProjectiles);

				HandleDPS(newProjectiles, shotsNum * projectilesPerShot, player.itemTime, AutoReuse);
			}

			if(itemToReset != null) {
				itemToReset.TurnToAir();
			}

			skipStartUse = true;
		}
		public virtual void StartUseChecks()
		{
			if(skipStartUseChecks) {
				return;
			}

			if(player.itemAnimation > 0 && player.itemAnimation > oPlayer.itemAnimPrev) {
				if(!skipStartUse) {
					StartUse(true);
				}
			} else if(player.itemAnimation <= 1) {
				skipStartUse = false;
			}
		}
		public virtual int GetDefaultShootProjectile(AmmoInfo? ammo)
		{
			return ammo?.shoot ?? item.shoot;
		}

		public override void Update()
		{
			//if(!inAltUse) {
			//	item.useTime = itemDefault.useTime;
			//	item.useAnimation = itemDefault.useAnimation;
			//	item.UseSound = itemDefault.UseSound;
			//	item.useStyle = itemDefault.useStyle;
			//	item.noMelee = itemDefault.noMelee;
			//}

			if(oPlayer.CanUseItems && player.itemAnimation <= 0 && CanStartUse && ItemCanUseItemEvent.Invoke(oItem, item, this, oPlayer, player)) {
				TryStartUse();
			}

			StartUseChecks();
		}
		public override bool CanUseItem(Item item, Player player) => player.altFunctionUse != 0;
		public override bool ConsumeItem() => allowConsume || player.altFunctionUse != 0;

		public static void HandleDPS(List<Projectile> list, int totalShotsNum, int useTime, bool itemHasAutoReuse)
		{
			if(!itemHasAutoReuse) {
				useTime = Math.Max(10, useTime) + 5;
			}
			float dps = list.GatherInt(p => p.damage) / (float)totalShotsNum / (useTime * TimeSystem.LogicDeltaTime);

			foreach(var proj in list) {
				proj.GetOverhaulProjectile().ownerDPS = dps;
			}
		}

		[HookBind(typeof(OverhaulProjectiles), nameof(OverhaulProjectiles.HookProjectileCreated))]
		protected static void ProjectileCreated_AdvancedItem(Projectile proj)
		{
			if(shootingItem != null) {
				if(callCustomShoot) {
					shootingItem.CustomShoot(shotId, 0, ref proj.type, ref proj.position, ref proj.velocity, ref proj.damage, ref proj.knockBack, ref proj.ai[0], ref proj.ai[1]);
				}

				shootingItem.OnProjectileCreated(proj);
			}
		}*/
	}
}
