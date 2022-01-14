using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerItemRotation : ModPlayer
	{
		public float? forcedItemRotation;

		public override void PostUpdate()
		{
			if (forcedItemRotation.HasValue) {
				Player.itemRotation = forcedItemRotation.Value;

				forcedItemRotation = null;
			}
		}
	}
}
