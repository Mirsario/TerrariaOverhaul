using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ModEntities.Players.Packets;
using TerrariaOverhaul.Core.Systems.Networking;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerDirectioning : ModPlayer
	{
		private const int MouseWorldSyncFrequency = 12;

		public int forcedDirection;
		public Vector2 mouseWorld;

		private Vector2 lastSyncedMouseWorld;

		public override void Load()
		{
			On.Terraria.Player.HorizontalMovement += (orig, player) => {
				orig(player);

				player.GetModPlayer<PlayerDirectioning>()?.SetDirection();
			};
			On.Terraria.Player.ChangeDir += (orig, player, dir) => {
				orig(player, dir);

				player.GetModPlayer<PlayerDirectioning>()?.SetDirection();
			};
		}
		public override void PreUpdate() => SetDirection(true);
		public override void PostUpdate() => SetDirection();
		public override bool PreItemCheck()
		{
			SetDirection();

			return true;
		}

		public void SetDirection() => SetDirection(false);

		private void SetDirection(bool resetForcedDirection)
		{
			if(!Main.dedServ && Main.gameMenu) {
				Player.direction = 1;

				return;
			}

			if(Player.IsLocal() && Main.hasFocus) {
				mouseWorld = Main.MouseWorld;

				if(Main.netMode == NetmodeID.MultiplayerClient && Main.GameUpdateCount % MouseWorldSyncFrequency == 0 && lastSyncedMouseWorld != mouseWorld) {
					MultiplayerSystem.SendPacket(new PlayerMousePositionPacket(Player));

					lastSyncedMouseWorld = mouseWorld;
				}
			}

			if(!Player.pulley && (!Player.mount.Active || Player.mount.AllowDirectionChange) && (!Player.InItemAnimation || ICanTurnDuringItemUse.Hook.Invoke(Player.HeldItem, Player))) {
				if(forcedDirection != 0) {
					Player.direction = forcedDirection;

					if(resetForcedDirection) {
						forcedDirection = 0;
					}
				} else {
					Player.direction = mouseWorld.X >= Player.Center.X ? 1 : -1;
				}
			}
		}
	}
}
