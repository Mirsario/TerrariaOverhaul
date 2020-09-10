using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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

			if(player.IsLocal() && Main.hasFocus) {
				mouseWorld = Main.MouseWorld;

				if(Main.netMode==NetmodeID.MultiplayerClient && Main.GameUpdateCount%MouseWorldSyncFrequency==0 && lastSyncedMouseWorld!=mouseWorld) {
					MultiplayerSystem.SendPacket(new PlayerMousePositionPacket(player));

					lastSyncedMouseWorld = mouseWorld;
				}
			}

			if(!player.pulley && (!player.mount.Active || player.mount.AllowDirectionChange)) {
				if(forcedDirection!=0) {
					player.direction = forcedDirection;

					forcedDirection = 0;
				} else {
					player.direction = mouseWorld.X>=player.Center.X ? 1 : -1;
				}
			}
		}
	}
}
