using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Utilities
{
	//TODO: Somehow make this have conditional instancing?
	public class ItemCharging : GlobalItem
	{
		public delegate void ChargeEndAction(Item item, Player player, float chargeProgress);

		private ChargeEndAction action;

		public bool IsCharging { get; private set; }
		public int ChargeTime { get; private set; }
		public int ChargeTimeMax { get; private set; }

		public float ChargeProgress => IsCharging ? ChargeTime / (float)ChargeTimeMax : 0f;

		public override bool InstancePerEntity => true;

		public override GlobalItem Clone(Item item, Item itemClone) => base.Clone(item, itemClone);
		public override void HoldItem(Item item, Player player)
		{
			if(IsCharging) {
				ChargeTime++;

				if(ChargeTime >= ChargeTimeMax) {
					StopCharge(item, player);
				}
			}
		}

		public void StartCharge(int chargeTime, ChargeEndAction action)
		{
			IsCharging = true;
			ChargeTime = 0;
			ChargeTimeMax = chargeTime;

			this.action = action;
		}
		public void StopCharge(Item item, Player player, bool skipAction = false)
		{
			if(!skipAction) {
				action?.Invoke(item, player, ChargeProgress);
			}

			IsCharging = false;
			ChargeTime = ChargeTimeMax = 0;
			action = null;
		}
	}
}
