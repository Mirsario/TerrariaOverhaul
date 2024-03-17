using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerAutoJump : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableAutoJump = new(ConfigSide.Both, "Movement", nameof(EnableAutoJump), () => true) {
		ExtraCategories = new[] { "Accessibility" }
	};

	public override void ResetEffects()
	{
		if (EnableAutoJump) {
			Player.autoJump = true;
		}
	}
}
