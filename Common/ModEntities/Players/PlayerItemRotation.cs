namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerItemRotation : PlayerBase
	{
		public float? forcedItemRotation;

		public override void PostUpdate()
		{
			if(forcedItemRotation.HasValue) {
				Player.itemRotation = forcedItemRotation.Value;

				forcedItemRotation = null;
			}
		}
	}
}
