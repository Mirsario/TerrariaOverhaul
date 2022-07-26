using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Players;

public sealed class PlayerItemRotation : ModPlayer
{
	public float? ForcedItemRotation;

	public override void PostUpdate()
	{
		if (ForcedItemRotation.HasValue) {
			Player.itemRotation = ForcedItemRotation.Value;

			ForcedItemRotation = null;
		}
	}
}
