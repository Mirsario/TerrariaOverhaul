using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ModEntities.Players;

[Autoload(Side = ModSide.Client)]
public sealed class PlayerBleeding : ModPlayer
{
	public static readonly Gradient<float> BleedingEffectHealthGradient = new(
		(0f, 1f),
		(30f, 1f),
		(50f, 0f)
	);

	private float bleedingCounter;

	public override void PostUpdate()
	{
		if (!Player.dead) {
			return;
		}

		float bleedingIntensity = BleedingEffectHealthGradient.GetValue(Player.statLife);

		bleedingCounter += bleedingIntensity / 4f;

		while (bleedingCounter >= 1f) {
			Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Blood);

			bleedingCounter--;
		}
	}
}
