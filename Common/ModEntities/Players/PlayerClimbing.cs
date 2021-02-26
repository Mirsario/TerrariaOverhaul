using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.ModEntities.Players.Packets;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Systems.Networking;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerClimbing : ModPlayer
	{
		public bool forceClimb;
		public Timer climbCooldown;

		private Vector2 climbStartPos;
		private Vector2 climbEndPos;

		public float ClimbTime { get; private set; }
		public bool IsClimbing { get; private set; }

		public bool HasClimbingGear => Player.EnumerateAccessories().Any(tuple => OverhaulItemTags.ClimbingClaws.Has(tuple.item.type));

		public override bool PreItemCheck()
		{
			if(!IsClimbing) {
				TryStartClimbing();
			} else {
				UpdateClimbing();
			}

			return true;
		}

		public void StartClimbing(Vector2 posFrom, Vector2 posTo)
		{
			IsClimbing = true;
			ClimbTime = 0f;
			climbStartPos = Player.position = posFrom;
			climbEndPos = posTo;

			if(Main.netMode == NetmodeID.MultiplayerClient && Player.IsLocal()) {
				MultiplayerSystem.SendPacket(new PlayerClimbStartMessage(Player, posFrom, posTo));
			}
		}

		private void TryStartClimbing()
		{
			//isSleeping ||
			if(climbCooldown.Active) {
				return;
			}

			//Can only be started on local client
			if(!Player.IsLocal()) {
				return;
			}

			//Check for keypress.
			if(!Player.controlUp && !forceClimb) {
				return;
			}

			forceClimb = false;

			//Disable climbing if flying upwards or flying with wings
			if(Player.velocity.Y < -6f /* || (Player.wingsLogic > 0 && Player.wingTime > 0f)*/) {
				return;
			}

			if(Player.pulley || Player.EnumerateGrapplingHooks().Any() || (Player.mount != null && Player.mount.Active)) {
				return;
			}

			Player.GetModPlayer<PlayerDirectioning>().SetDirection();

			var tilePos = Player.position.ToTileCoordinates();

			for(int i = 1; i >= 0; i--) {
				var pos = new Point16(tilePos.X + (Player.direction == 1 ? 2 : -1), tilePos.Y + i);

				//The base tile has to be solid
				if(!Main.tile.TryGet(pos, out var tempTile) || !tempTile.IsActive || tempTile.IsActuated || (!Main.tileSolid[tempTile.type] && !Main.tileSolidTop[tempTile.type]) || (i != 0 && tempTile.Slope != SlopeID.Solid)) {
					continue;
				}

				//Ice can't climbed on, unless you have climbing gear
				if(OverhaulTileTags.NoClimbing.Has(tempTile.type) && !HasClimbingGear) {
					continue;
				}

				bool CheckFree(int x, int y, Tile t) => !(t.IsActive && !t.IsActuated) || !Main.tileSolid[t.type] || OverhaulTileTags.AllowClimbing.Has(t.type);

				if(!(
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

			Player.gfxOffY = 0f; //Disable autostep vertical sprite offsets.

			//If we don't reset fall time, the player may explode from fall damage once they stop climbing.
			Player.fallStart = (int)(Player.position.Y / 16f);

			//Force direction.
			playerDirectioning.forcedDirection = climbStartPos.X <= climbEndPos.X ? 1 : -1;

			//Progress climbing.
			ClimbTime = MathUtils.StepTowards(ClimbTime, 1f, (HasClimbingGear ? 3.5f : 2f) * TimeSystem.LogicDeltaTime);

			if(ClimbTime >= 1f) {
				IsClimbing = false;
			} else {
				playerAnimations.forcedBodyFrame = ClimbTime > 0.75f ? PlayerFrames.Use4 : ClimbTime > 0.5f ? PlayerFrames.Use3 : ClimbTime > 0.25f ? PlayerFrames.Use2 : PlayerFrames.Use1;
				playerRotation.rotation = ClimbTime * 0.7f * Player.direction;
				playerRotation.rotationOffsetScale = 0f;
			}

			Player.position.X = MathHelper.Lerp(climbStartPos.X, climbEndPos.X, ClimbTime);
			Player.position.Y = ClimbTime < 0.75f ? MathHelper.Lerp(climbStartPos.Y, climbEndPos.Y, ClimbTime / 0.75f) : climbEndPos.Y;
			playerMovement.forcedPosition = Player.position;

			//Fix suffocating when climbing sand
			Player.suffocating = false;
			Player.suffocateDelay = 0;

			//Make sure we're not moving
			Player.jump = 0;
			Player.velocity = new Vector2(0f, 0.0001f);
			Player.runAcceleration = 0f;
			Player.runSlowdown = 0f;
			Player.maxRunSpeed = 0f;

			//Prevent other actions
			Player.GetModPlayer<PlayerDodgerolls>().dodgeCooldown.Set(1);
		}
	}
}
