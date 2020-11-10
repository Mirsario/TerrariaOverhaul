namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerItemRotation : PlayerBase
	{
		public float? forcedItemRotation;

		public override void PostUpdate()
		{
			if(forcedItemRotation.HasValue) {
				player.itemRotation = forcedItemRotation.Value;

				forcedItemRotation = null;
			}
		}
	}
}
