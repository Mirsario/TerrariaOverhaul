using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Components;
using TerrariaOverhaul.Common.Camera.ScreenShakes;
using TerrariaOverhaul.Content.Gores;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Guns
{
	public class Shotgun : Gun
	{
		private uint pumpTime;

		public ISoundStyle PumpSound { get; set; }
		public int ShellCount { get; set; } = 1;

		public override bool ShouldApplyItemOverhaul(Item item) => item.useAmmo == AmmoID.Bullet && (item.UseSound == SoundID.Item36 || item.UseSound == SoundID.Item38);

		public override void SetDefaults(Item item)
		{
			item.UseSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Shotgun/ShotgunFire", 4, volume: 0.2f, pitchVariance: 0.2f);
			PumpSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Shotgun/ShotgunPump", 0, volume: 0.25f, pitchVariance: 0.1f);

			ShellCount = item.type switch {
				ItemID.Boomstick => 2,
				ItemID.QuadBarrelShotgun => 4,
				_ => 1,
			};

			if (!Main.dedServ) {
				item.EnableComponent<ItemUseVisualRecoil>(c => {
					c.Power = 25f;
				});

				item.EnableComponent<ItemUseScreenShake>(c => {
					c.ScreenShake = new ScreenShake(10f, 0.25f);
				});
			}
		}

		public override bool? UseItem(Item item, Player player)
		{
			bool? baseResult = base.UseItem(item, player);

			if (baseResult == false) {
				return false;
			}

			pumpTime = (uint)(Main.GameUpdateCount + player.itemAnimationMax / 2);

			return baseResult;
		}

		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			if (!Main.dedServ && PumpSound != null && pumpTime != 0 && Main.GameUpdateCount == pumpTime) {
				SoundEngine.PlaySound(PumpSound, player.Center);
				SpawnCasings<ShellCasing>(player, ShellCount);
			}
		}
	}
}
