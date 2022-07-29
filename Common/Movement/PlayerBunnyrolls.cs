using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Dodgerolls;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerBunnyrolls : ModPlayer, IPlayerOnBunnyhopHook
{
	public void OnBunnyhop(Player player, ref float boost)
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

			SoundEngine.PlaySound(SoundID.DoubleJump, playerCenter);
		}
	}
}
