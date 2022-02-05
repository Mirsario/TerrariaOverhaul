using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Players.Packets;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Systems.Configuration;
using TerrariaOverhaul.Core.Systems.Networking;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerClimbing : ModPlayer
	{
		public static readonly ConfigEntry<bool> EnableClimbing = new(ConfigSide.Both, "PlayerMovement", nameof(EnableClimbing), () => true);

		public bool forceClimb;
		public Timer climbCooldown;

		private Vector2 climbStartPos;
		private Vector2 climbEndPos;

		public float ClimbProgress { get; private set; }
		public bool IsClimbing { get; private set; }

		public bool HasClimbingGear => Player.EnumerateAccessories().Any(tuple => OverhaulItemTags.ClimbingClaws.Has(tuple.item.type));
		public float ClimbTime => HasClimbingGear ? 0.125f : 0.25f;

		public override bool PreItemCheck()
		{
			if (!IsClimbing) {
				TryStartClimbing();
			} else {
				UpdateClimbing();
			}

			return true;
		}

		public void StartClimbing(Vector2 posFrom, Vector2 posTo)
		{
			IsClimbing = true;
			ClimbProgress = 0f;
			climbStartPos = Player.position = posFrom;
			climbEndPos = posTo;

			if (Main.netMode == NetmodeID.MultiplayerClient && Player.IsLocal()) {
				MultiplayerSystem.SendPacket(new PlayerClimbStartMessage(Player, posFrom, posTo));
			}
		}

		private void TryStartClimbing()
		{
			if (!EnableClimbing) {
				return;
			}

			//isSleeping ||
			if (climbCooldown.Active) {
				return;
			}

			// Can only be started on local client
			if (!Player.IsLocal()) {
				return;
			}

			// Check for keypress.
			if (!Player.controlUp && !forceClimb) {
				return;
			}

			forceClimb = false;

			// Disable climbing if flying upwards or flying with wings
			if (Player.velocity.Y < -6f /* || (Player.wingsLogic > 0 && Player.wingTime > 0f)*/) {
				return;
			}

			if (Player.pulley || Player.EnumerateGrapplingHooks().Any() || (Player.mount != null && Player.mount.Active)) {
				return;
			}

			Player.GetModPlayer<PlayerDirectioning>().SetDirection();

			var tilePos = Player.position.ToTileCoordinates();

			for (int i = 1; i >= 0; i--) {
				var pos = new Point16(tilePos.X + (Player.direction == 1 ? 2 : -1), tilePos.Y + i);

				// The base tile has to be solid
				if (!Main.tile.TryGet(pos, out var tile) || !tile.HasUnactuatedTile || (!Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]) || (i != 0 && tile.Slope != SlopeType.Solid)) {
					continue;
				}

				// Ice can't climbed on, unless you have climbing gear
				if (OverhaulTileTags.NoClimbing.Has(tile.TileType) && !HasClimbingGear) {
					continue;
				}

				bool CheckFree(int x, int y, Tile t)
					=> !(t.HasTile && !t.IsActuated) || !Main.tileSolid[t.TileType] || OverhaulTileTags.AllowClimbing.Has(t.TileType);

				if (!(
					TileCheckUtils.CheckAreaAll(pos.X, pos.Y - 3, 1, 3, CheckFree)
					& TileCheckUtils.CheckAreaAll(pos.X + (Player.direction == 1 ? -1 : 1), pos.Y - 3, 1, 4, CheckFree)
					& TileCheckUtils.CheckAreaAll(pos.X + (Player.direction == 1 ? -2 : 2), pos.Y - 2, 1, 3, CheckFree)
				)) {
					continue;
				}

				StartClimbing(Player.position, new Vector2(pos.X * 16f + (Player.direction == 1 ? -4f : 0f), (pos.Y - 3) * 16f + 6));
				UpdateClimbing();
				break;
			}
		}
		private void UpdateClimbing()
		{
			var playerMovement = Player.GetModPlayer<PlayerMovement>();
			var playerRotation = Player.GetModPlayer<PlayerRotation>();
			var playerAnimations = Player.GetModPlayer<PlayerAnimations>();
			var playerDirectioning = Player.GetModPlayer<PlayerDirectioning>();

			Player.gfxOffY = 0f; // Disable autostep vertical sprite offsets.

			// If we don't reset fall time, the player may explode from fall damage once they stop climbing.
			Player.fallStart = (int)(Player.position.Y / 16f);

			// Force direction.
			playerDirectioning.forcedDirection = climbStartPos.X <= climbEndPos.X ? 1 : -1;

			// Progress climbing.
			ClimbProgress = MathUtils.StepTowards(ClimbProgress, 1f, (1f / ClimbTime) * TimeSystem.LogicDeltaTime);

			if (ClimbProgress >= 1f) {
				IsClimbing = false;
			} else {
				playerAnimations.forcedBodyFrame = ClimbProgress > 0.75f ? PlayerFrames.Use4 : ClimbProgress > 0.5f ? PlayerFrames.Use3 : ClimbProgress > 0.25f ? PlayerFrames.Use2 : PlayerFrames.Use1;
				playerRotation.rotation = ClimbProgress * 0.7f * Player.direction;
				playerRotation.rotationOffsetScale = 0f;
			}

			Player.position.X = MathHelper.Lerp(climbStartPos.X, climbEndPos.X, ClimbProgress);
			Player.position.Y = ClimbProgress < 0.75f ? MathHelper.Lerp(climbStartPos.Y, climbEndPos.Y, ClimbProgress / 0.75f) : climbEndPos.Y;
			playerMovement.forcedPosition = Player.position;

			// Fix suffocating when climbing sand
			Player.suffocating = false;
			Player.suffocateDelay = 0;

			// Make sure we're not moving
			Player.jump = 0;
			Player.velocity = new Vector2(0f, 0.0001f);
			Player.runAcceleration = 0f;
			Player.runSlowdown = 0f;
			Player.maxRunSpeed = 0f;

			// Prevent other actions
			Player.GetModPlayer<PlayerDodgerolls>().dodgeCooldown.Set(1);
		}
	}
}
