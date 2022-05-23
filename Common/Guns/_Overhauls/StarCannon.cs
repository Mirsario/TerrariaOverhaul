using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace TerrariaOverhaul.Common.Guns
{
	public class StarCannon : Minigun
	{
		public static readonly SoundStyle RocketLauncherFireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/StarCannon/StarCannonFire") {
			Volume = 0.2f,
			PitchVariance = 0.2f,
		};

		public override bool ShouldApplyItemOverhaul(Item item)
		{
			if (item.useAmmo != AmmoID.FallenStar) {
				return false;
			}

			if (item.UseSound != null && item.UseSound != SoundID.Item9) {
				return false;
			}

			return true;
		}

		public override void SetDefaults(Item item)
		{
			base.SetDefaults(item);

			item.UseSound = RocketLauncherFireSound;
		}
	}
}
