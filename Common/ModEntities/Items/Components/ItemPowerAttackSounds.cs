using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ItemPowerAttackSounds : ItemComponent
	{
		public ISoundStyle Sound;
		public bool CancelPlaybackOnEnd;
		public float RequiredChargeProgress;

		private SlotId soundInstance;
		private float previousChargeProgress;

		public override void SetDefaults(Item item)
		{
			if (item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks)) {
				powerAttacks.OnChargeStart += OnChargeStart;
				powerAttacks.OnChargeUpdate += OnChargeUpdate;
				powerAttacks.OnChargeEnd += OnChargeEnd;
			}
		}

		public override void HoldItem(Item item, Player player)
		{
			if (!Main.dedServ && soundInstance.IsValid) {
				var activeSound = SoundEngine.GetActiveSound(soundInstance);

				if (activeSound != null) {
					activeSound.Position = player.Center;
				} else {
					soundInstance = default;
				}
			}
		}

		private void PlaySound(Player player)
		{
			soundInstance = SoundEngine.PlayTrackedSound(Sound, player.Center);
		}

		private static void OnChargeStart(Item item, Player player, float chargeLength)
		{
			var instance = item.GetGlobalItem<ItemPowerAttackSounds>();

			if (instance.Enabled && instance.RequiredChargeProgress == 0f) {
				instance.PlaySound(player);
			}
		}

		private static void OnChargeUpdate(Item item, Player player, float chargeLength, float progress)
		{
			var instance = item.GetGlobalItem<ItemPowerAttackSounds>();

			if (instance.Enabled) {
				if (instance.RequiredChargeProgress > 0f && progress > instance.RequiredChargeProgress && instance.previousChargeProgress <= instance.RequiredChargeProgress) {
					instance.PlaySound(player);
				}

				instance.previousChargeProgress = progress;
			}
		}

		private static void OnChargeEnd(Item item, Player player, float chargeLength, float progress)
		{
			var instance = item.GetGlobalItem<ItemPowerAttackSounds>();

			if (instance.Enabled && instance.CancelPlaybackOnEnd && instance.soundInstance.IsValid) {
				SoundEngine.GetActiveSound(instance.soundInstance)?.Stop();
			}
		}
	}
}
