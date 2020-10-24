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
using TerrariaOverhaul.Utilities.Enums;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public class PlayerClimbing : ModPlayer
	{
		public bool forceClimb;
		public bool noClimbing;

		private Vector2 climbStartPos;
		private Vector2 climbEndPos;

		public float ClimbTime { get; private set; }
		public bool IsClimbing { get; private set; }

		public bool HasClimbingGear => player.EnumerateAccessories().Any(tuple => OverhaulItemTags.ClimbingClaws.Has(tuple.item.type));

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
			climbStartPos = player.position = posFrom;
			climbEndPos = posTo;

			player.GetModPlayer<PlayerDirectioning>().forcedDirection = climbEndPos.X >= climbStartPos.X ? 1 : -1;

			if(Main.netMode == NetmodeID.MultiplayerClient && player.IsLocal()) {
				MultiplayerSystem.SendPacket(new PlayerClimbStartMessage(player, posFrom, posTo));
			}
		}

		private void TryStartClimbing()
		{
			//isDodging || forceDodgeroll || isSleeping || 
			if(noClimbing) {
				noClimbing = false;

				return;
			}

			//Can only be started on local client
			if(!player.IsLocal()) {
				return;
			}

			//Check for keypress.
			if(!player.controlUp && !forceClimb) {
				return;
			}

			forceClimb = false;

			//Disable climbing if flying upwards or flying with wings
			if(player.velocity.Y < -6f || (player.wingsLogic > 0 && player.wingTime > 0f)) {
				return;
			}

			if(player.pulley || player.EnumerateGrapplingHooks().Any() || (player.mount != null && player.mount.Active)) {
				return;
			}

			player.GetModPlayer<PlayerDirectioning>().SetDirection();

			var tilePos = player.position.ToTileCoordinates();

			for(int i = 1; i >= 0; i--) {
				var pos = new Point16(tilePos.X + (player.direction == 1 ? 2 : -1), tilePos.Y + i);

				//The base tile has to be solid
				if(!Main.tile.TryGet(pos, out var tempTile) || !tempTile.active() || tempTile.inActive() || (!Main.tileSolid[tempTile.type] && !Main.tileSolidTop[tempTile.type]) || (i != 0 && tempTile.slope() != 0)) {
					continue;
				}

				//Ice can't climbed on, unless you have climbing gear
				if(OverhaulTileTags.NoClimbing.Has(tempTile.type) && !HasClimbingGear) {
					continue;
				}

				bool Check(int x, int y, Tile t) => !t.nactive() || !Main.tileSolid[t.type] || OverhaulTileTags.AllowClimbing.Has(t.type);

				if(!(
					TileCheckUtils.CheckAreaAll(pos.X, pos.Y - 3, 1, 3, Check)
					& TileCheckUtils.CheckAreaAll(pos.X + (player.direction == 1 ? -1 : 1), pos.Y - 3, 1, 4, Check)
					& TileCheckUtils.CheckAreaAll(pos.X + (player.direction == 1 ? -2 : 2), pos.Y - 2, 1, 3, Check)
				)) {
					continue;
				}

				StartClimbing(player.position, new Vector2(pos.X * 16f + (player.direction == 1 ? -4f : 0f), (pos.Y - 3) * 16f + 6));
				break;
			}
		}
		private void UpdateClimbing()
		{
			var playerMovement = player.GetModPlayer<PlayerMovement>();
			var playerRotation = player.GetModPlayer<PlayerRotation>();
			var playerAnimations = player.GetModPlayer<PlayerAnimations>();
			var playerDirectioning = player.GetModPlayer<PlayerDirectioning>();

			player.gfxOffY = 0f; //Disable autostep vertical sprite offsets.

			//If we don't reset fall time, the player may explode from fall damage once they stop climbing.
			player.fallStart = (int)(player.position.Y / 16f);

			//Force direction.
			playerDirectioning.forcedDirection = climbStartPos.X <= climbEndPos.X ? 1 : -1;

			//Progress climbing.
			ClimbTime = MathUtils.StepTowards(ClimbTime, 1f, (HasClimbingGear ? 3.5f : 2f) * TimeSystem.LogicDeltaTime);

			if(ClimbTime >= 1f) {
				IsClimbing = false;
			} else {
				playerAnimations.forcedBodyFrame = ClimbTime > 0.75f ? PlayerFrames.Use4 : ClimbTime > 0.5f ? PlayerFrames.Use3 : ClimbTime > 0.25f ? PlayerFrames.Use2 : PlayerFrames.Use1;
				playerRotation.rotation = ClimbTime * 0.7f * player.direction;
				playerRotation.rotationOffsetScale = 0f;
			}

			player.position.X = MathHelper.Lerp(climbStartPos.X, climbEndPos.X, ClimbTime);
			player.position.Y = ClimbTime < 0.75f ? MathHelper.Lerp(climbStartPos.Y, climbEndPos.Y, ClimbTime / 0.75f) : climbEndPos.Y;
			playerMovement.forcedPosition = player.position;

			//Fix suffocating when climbing sand
			player.suffocating = false;
			player.suffocateDelay = 0;

			//Make sure we're not moving
			player.jump = 0;
			player.velocity = new Vector2(0f, 0.0001f);
			player.runAcceleration = 0f;
			player.runSlowdown = 0f;
			player.maxRunSpeed = 0f;
		}
	}
}
