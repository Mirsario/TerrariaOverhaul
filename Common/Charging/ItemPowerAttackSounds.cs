using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Charging
{
	[Autoload(Side = ModSide.Client)]
	public sealed class ItemPowerAttackSounds : ItemComponent, IModifyItemUseSound
	{
		public SoundStyle? Sound;
		public bool CancelPlaybackOnEnd;
		public float RequiredChargeProgress;
		public bool ReplacesUseSound;

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
				if (SoundEngine.TryGetActiveSound(soundInstance, out var activeSound)) {
					activeSound.Position = player.Center;
				} else {
					soundInstance = default;
				}
			}
		}

		void IModifyItemUseSound.ModifyItemUseSound(Item item, Player player, ref SoundStyle? useSound)
		{
			if (ReplacesUseSound && item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) && powerAttacks.Enabled && powerAttacks.PowerAttack) {
				useSound = Sound;
			}
		}

		private void PlaySound(Player player)
		{
			if (Sound.HasValue) {
				soundInstance = SoundEngine.PlaySound(Sound.Value, player.Center);
			}
		}

		private static void OnChargeStart(Item item, Player player, float chargeLength)
		{
			var instance = item.GetGlobalItem<ItemPowerAttackSounds>();

			if (!instance.ReplacesUseSound && instance.Enabled && instance.RequiredChargeProgress == 0f) {
				instance.PlaySound(player);
			}
		}

		private static void OnChargeUpdate(Item item, Player player, float chargeLength, float progress)
		{
			var instance = item.GetGlobalItem<ItemPowerAttackSounds>();

			if (instance.Enabled) {
				if (!instance.ReplacesUseSound && instance.RequiredChargeProgress > 0f && progress > instance.RequiredChargeProgress && instance.previousChargeProgress <= instance.RequiredChargeProgress) {
					instance.PlaySound(player);
				}

				instance.previousChargeProgress = progress;
			}
		}

		private static void OnChargeEnd(Item item, Player player, float chargeLength, float progress)
		{
			var instance = item.GetGlobalItem<ItemPowerAttackSounds>();

			if (instance.Enabled && instance.CancelPlaybackOnEnd && SoundEngine.TryGetActiveSound(instance.soundInstance, out var activeSound)) {
				activeSound.Stop();
			}
		}
	}
}
