using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerBunnyrolls : ModPlayer, IPlayerOnBunnyhopHook
{
	public static readonly SoundStyle BunnyrollSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/Bunnyroll") {
		Volume = 0.9f,
		PitchVariance = 0.2f,
	};
	
	public void OnBunnyhop(Player player, ref float boost, ref float boostMultiplier)
	{
		if (!Player.TryGetModPlayer(out PlayerDodgerolls dodgerolls) || !dodgerolls.IsDodging) {
			return;
		}
		
		boost += 0.5f;

		if (!Main.dedServ) {
			var playerCenter = Player.Center;
			var entitySource = Player.GetSource_FromThis();

			for (int i = 0; i < 3; i++) {
				var position = playerCenter + new Vector2(Main.rand.NextFloat(-4f, 4f), 0f);
				var velocity = new Vector2((i - 1) * 0.9f, -0.1f);

				Gore.NewGorePerfect(entitySource, position, velocity, GoreID.Smoke1 + i);
			}

			SoundEngine.PlaySound(BunnyrollSound.WithVolumeScale(Player.IsLocal() ? 1f : 0.5f), playerCenter);
		}
	}
}
