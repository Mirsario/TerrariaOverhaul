using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;

namespace TerrariaOverhaul.Common.Guns;

public class Shotgun : ItemOverhaul
{
	public static readonly SoundStyle ShotgunFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Shotgun/ShotgunFire", 4) {
		Volume = 0.2f,
		PitchVariance = 0.2f
	};
	public static readonly SoundStyle ShotgunPumpSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/Shotgun/ShotgunPump") {
		Volume = 0.25f,
		PitchVariance = 0.1f
	};

	private uint pumpTime;

	public SoundStyle? PumpSound { get; set; }
	public int ShellCount { get; set; } = 1;

	public override bool ShouldApplyItemOverhaul(Item item)
		=> item.useAmmo == AmmoID.Bullet && (item.UseSound == SoundID.Item36 || item.UseSound == SoundID.Item38);

	public override void SetDefaults(Item item)
	{
		item.UseSound = ShotgunFireSound;
		PumpSound = ShotgunPumpSound;

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
			item.EnableComponent<ItemMuzzleflashes>();

			item.EnableComponent<ItemBulletCasings>(c => {
				c.CasingGoreType = ModContent.GoreType<ShellCasing>();
				c.CasingCount = item.type switch {
					ItemID.Boomstick => 2,
					ItemID.QuadBarrelShotgun => 4,
					_ => 1,
				};
				c.SpawnOnUse = false;
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
			SoundEngine.PlaySound(PumpSound.Value, player.Center);

			item.GetGlobalItem<ItemBulletCasings>().SpawnCasings(item, player);
		}
	}
}
