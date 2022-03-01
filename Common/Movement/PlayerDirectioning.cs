﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.ModEntities.Players;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement
{
	public sealed class PlayerDirectioning : ModPlayer
	{
		private const int MouseWorldSyncFrequency = 12;

		private Vector2 lastSyncedMouseWorld;

		public int ForcedDirection { get; set; }
		public Vector2 MouseWorld { get; set; }

		public override void Load()
		{
			On.Terraria.Player.HorizontalMovement += static (orig, player) => {
				orig(player);

				player.GetModPlayer<PlayerDirectioning>()?.SetDirection();
			};

			On.Terraria.Player.ChangeDir += static (orig, player, dir) => {
				orig(player, dir);

				player.GetModPlayer<PlayerDirectioning>()?.SetDirection();
			};
		}

		public override void PreUpdate()
			=> SetDirection(true);

		public override void PostUpdate()
			=> SetDirection();

		public override bool PreItemCheck()
		{
			SetDirection();

			return true;
		}

		public void SetDirection()
			=> SetDirection(false);

		private void SetDirection(bool resetForcedDirection)
		{
			if (!Main.dedServ && Main.gameMenu) {
				Player.direction = 1;

				return;
			}

			if (Player.IsLocal() && Main.hasFocus) {
				MouseWorld = Main.MouseWorld;

				if (Main.netMode == NetmodeID.MultiplayerClient && Main.GameUpdateCount % MouseWorldSyncFrequency == 0 && lastSyncedMouseWorld != MouseWorld) {
					MultiplayerSystem.SendPacket(new PlayerMousePositionPacket(Player));

					lastSyncedMouseWorld = MouseWorld;
				}
			}

			if (!Player.pulley && (!Player.mount.Active || Player.mount.AllowDirectionChange) && (Player.itemAnimation <= 1 || ICanTurnDuringItemUse.Hook.Invoke(Player.HeldItem, Player))) {
				if (ForcedDirection != 0) {
					Player.direction = ForcedDirection;

					if (resetForcedDirection) {
						ForcedDirection = 0;
					}
				} else {
					Player.direction = MouseWorld.X >= Player.Center.X ? 1 : -1;
				}
			}
		}
	}
}
