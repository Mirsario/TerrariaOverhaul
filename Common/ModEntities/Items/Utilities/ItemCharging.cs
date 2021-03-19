using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Items.Hooks;

namespace TerrariaOverhaul.Common.ModEntities.Items.Utilities
{
	//TODO: Somehow make this have conditional instancing?
	public class ItemCharging : GlobalItem, ICanTurnDuringItemUse
	{
		public delegate void ChargeAction(Item item, Player player, float chargeProgress);

		private ChargeAction updateAction;
		private ChargeAction endAction;
		private bool? allowTurning;

		public bool IsCharging { get; private set; }
		public int ChargeTime { get; private set; }
		public int ChargeTimeMax { get; private set; }

		public float ChargeProgress => IsCharging ? ChargeTime / (float)ChargeTimeMax : 0f;

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone) => base.Clone(item, itemClone);
		public override void HoldItem(Item item, Player player)
		{
			if(IsCharging) {
				updateAction?.Invoke(item, player, ChargeProgress);

				ChargeTime++;

				if(ChargeTime >= ChargeTimeMax) {
					StopCharge(item, player);
				}
			}
		}

		public void StartCharge(int chargeTime, ChargeAction updateAction, ChargeAction endAction, bool? allowTurning = null)
		{
			IsCharging = true;
			ChargeTime = 0;
			ChargeTimeMax = chargeTime;

			this.updateAction = updateAction;
			this.endAction = endAction;
			this.allowTurning = allowTurning;
		}
		public void StopCharge(Item item, Player player, bool skipAction = false)
		{
			if(!skipAction) {
				endAction?.Invoke(item, player, ChargeProgress);
			}

			IsCharging = false;
			ChargeTime = ChargeTimeMax = 0;
			updateAction = null;
			endAction = null;
		}

		public bool? CanTurnDuringItemUse(Item item, Player player) => allowTurning;
	}
}
