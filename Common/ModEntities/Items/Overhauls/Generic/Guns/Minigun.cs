using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Camera.ScreenShakes;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Generic.Guns
{
	public class Minigun : Gun
	{
		private float speedFactor;

		public virtual float MinSpeedFactor => 0.33f;
		public virtual float AccelerationSpeed => 0.5f;
		public virtual float DecelerationSpeed => 2f;
		public virtual bool DoSpawnCasings => true;

		public override float OnUseVisualRecoil => 5f;
		public override ScreenShake OnUseScreenShake => new ScreenShake(5f, 0.25f);

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if(item.UseSound != SoundID.Item11 && item.UseSound != SoundID.Item40 && item.UseSound != SoundID.Item41) {
				return false;
			}

			//Exclude slow firing guns.
			if(item.useTime >= 10) {
				return false;
			}

			return true;
		}
		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Items/Guns/Minigun/MinigunFire", 0, volume: 0.15f, pitchVariance: 0.2f);
			speedFactor = MinSpeedFactor;
			PlaySoundOnEveryUse = true;
		}
		public override float UseTimeMultiplier(Item item, Player player)
		{
			return speedFactor; //Fire rate shenanigans.
		}
		public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage, ref float flat)
		{
			damage *= 1.05f; //A compensation for fire rate shenanigans.
		}
		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			if(player.controlUseItem) {
				speedFactor = MathUtils.StepTowards(speedFactor, 1f, AccelerationSpeed * TimeSystem.LogicDeltaTime);
			} else {
				speedFactor = MathUtils.StepTowards(speedFactor, MinSpeedFactor, DecelerationSpeed * TimeSystem.LogicDeltaTime);
			}
		}
		public override bool? UseItem(Item item, Player player)
		{
			if(!Main.dedServ && DoSpawnCasings) {
				SpawnCasings<BulletCasing>(player);
			}

			return base.UseItem(item, player);
		}
	}
}
