using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerDirectioning : ModPlayer
	{
		public int forcedDirection;
		public Vector2 mouseWorld;

		public override void PreUpdate() => SetDirection();
		public override void PostUpdate() => SetDirection();
		public override bool PreItemCheck()
		{
			SetDirection();

			return true;
		}

		public void SetDirection()
		{
			if(player.altFunctionUse!=0) {
				return;
			}

			if(!Main.dedServ && Main.gameMenu) {
				player.direction = 1;

				return;
			}

			if(player.IsLocal()) {
				mouseWorld = Main.MouseWorld;
			}

			if(!player.pulley && (!player.mount.Active || player.mount.AllowDirectionChange)) {
				if(forcedDirection!=0) {
					player.direction = forcedDirection;
				} else {
					player.direction = mouseWorld.X>=player.Center.X ? 1 : -1;
				}
			}
		}
	}
}
