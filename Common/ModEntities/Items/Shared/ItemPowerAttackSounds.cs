using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Shared
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ItemPowerAttackSounds : GlobalItem
	{
		public SoundStyle Sound;

		private SlotId chargeSoundInstance;

		public bool Enabled { get; set; }

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone) => base.Clone(item, itemClone);

		public override void SetDefaults(Item item)
		{
			if(item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks)) {
				powerAttacks.OnChargeStart += OnChargeStart;
				powerAttacks.OnChargeEnd += OnChargeEnd;
			}
		}

		public override void HoldItem(Item item, Player player)
		{
			if(!Main.dedServ && chargeSoundInstance.IsValid) {
				var activeSound = SoundEngine.GetActiveSound(chargeSoundInstance);

				if(activeSound != null) {
					activeSound.Position = player.Center;
				} else {
					chargeSoundInstance = default;
				}
			}
		}

		private static void OnChargeStart(Item item, Player player, float chargeLength)
		{
			var instance = item.GetGlobalItem<ItemPowerAttackSounds>();

			if(instance.Enabled) {
				instance.chargeSoundInstance = SoundEngine.PlayTrackedSound(instance.Sound, player.Center);
			}
		}

		private static void OnChargeEnd(Item item, Player player, float chargeLength, float progress)
		{
			var instance = item.GetGlobalItem<ItemPowerAttackSounds>();

			if(instance.Enabled && instance.chargeSoundInstance.IsValid) {
				SoundEngine.GetActiveSound(instance.chargeSoundInstance)?.Stop();
			}
		}
	}
}
