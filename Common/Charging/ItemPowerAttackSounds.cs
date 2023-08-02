using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Charging;

[Autoload(Side = ModSide.Client)]
public sealed class ItemPowerAttackSounds : ItemComponent, IModifyItemUseSound
{
	public SoundStyle? Sound;
	public bool CancelPlaybackOnEnd;
	public float RequiredChargeProgress;
	public bool ReplacesUseSound;

	private Timer lastCharge;
	private SlotId soundInstance;
	private float previousChargeProgress;

	public override void HoldItem(Item item, Player player)
	{
		if (!Enabled || !item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks)) {
			return;
		}

		var charge = powerAttacks.Charge;

		// React to charging
		if (charge.Active) {
			if (charge != lastCharge) {
				OnChargeStart(player);

				lastCharge = charge;
			}

			OnChargeUpdate(player, powerAttacks);
		} else if (charge.Progress != previousChargeProgress) {
			OnChargeEnd();
		}

		// Update sound position
		if (soundInstance.IsValid) {
			if (SoundEngine.TryGetActiveSound(soundInstance, out var activeSound)) {
				activeSound.Position = player.Center;
			} else {
				soundInstance = default;
			}
		}
	}

	void IModifyItemUseSound.ModifyItemUseSound(Item item, Player player, ref SoundStyle? useSound)
	{
		if (Enabled && ReplacesUseSound && item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) && powerAttacks.Enabled && powerAttacks.PowerAttack) {
			useSound = Sound;
		}
	}

	private void OnChargeStart(Player player)
	{
		if (!ReplacesUseSound && RequiredChargeProgress == 0f) {
			PlaySound(player);
		}
	}

	private void OnChargeUpdate(Player player, ItemPowerAttacks powerAttacks)
	{
		float progress = powerAttacks.Charge.Progress;

		if (!ReplacesUseSound && RequiredChargeProgress > 0f && progress > RequiredChargeProgress && previousChargeProgress <= RequiredChargeProgress) {
			PlaySound(player);
		}

		previousChargeProgress = progress;
	}

	private void OnChargeEnd()
	{
		if (Enabled && CancelPlaybackOnEnd && SoundEngine.TryGetActiveSound(soundInstance, out var activeSound)) {
			activeSound.Stop();
		}
	}

	private void PlaySound(Player player)
	{
		if (Sound.HasValue) {
			soundInstance = SoundEngine.PlaySound(Sound.Value, player.Center);
		}
	}
}
