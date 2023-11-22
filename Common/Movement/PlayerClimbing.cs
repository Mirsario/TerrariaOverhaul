using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerClimbing : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableClimbing = new(ConfigSide.Both, "PlayerMovement", nameof(EnableClimbing), () => true);

	private Vector2 climbStartPos;
	private Vector2 climbStartVelocity;
	private Vector2 climbEndPos;

	public bool ForceClimb;
	public Timer ClimbCooldown;

	public float ClimbProgress { get; private set; }
	public bool IsClimbing { get; private set; }

	public bool HasClimbingGear => Player.EnumerateAccessories().Any(tuple => OverhaulItemTags.ClimbingClaws.Has(tuple.item.type));
	public float ClimbTime => HasClimbingGear ? 0.133f : 0.175f;

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

		climbStartVelocity = new Vector2(
			MathUtils.MaxAbs(Player.velocity.X, Player.oldVelocity.X),
			MathUtils.MaxAbs(Player.velocity.Y, Player.oldVelocity.Y)
		);

		//climbEndPos.Y += climbStartVelocity.Y;

		if (Main.netMode == NetmodeID.MultiplayerClient && Player.IsLocal()) {
			MultiplayerSystem.SendPacket(new PlayerClimbStartPacket(Player, posFrom, posTo));
		}
	}

	private void TryStartClimbing()
	{
		if (!EnableClimbing) {
			return;
		}

		//isSleeping ||
		if (ClimbCooldown.Active) {
			return;
		}

		// Can only be started on local client
		if (!Player.IsLocal()) {
			return;
		}

		// Check for keypress.
		if (!Player.controlUp && !ForceClimb) {
			return;
		}

		ForceClimb = false;

		// Disable climbing if flying upwards or flying with wings
		if (Player.velocity.Y < -6f /* || (Player.wingsLogic > 0 && Player.wingTime > 0f)*/) {
			return;
		}

		if (Player.pulley || Player.EnumerateGrapplingHooks().Any() || Player.mount != null && Player.mount.Active) {
			return;
		}

		Player.GetModPlayer<PlayerDirectioning>().UpdateDirection();

		var tilePos = Player.position.ToTileCoordinates();

		for (int i = 1; i >= 0; i--) {
			var pos = new Point16(tilePos.X + (Player.direction == 1 ? 2 : -1), tilePos.Y + i);

			// The base tile has to be solid
			if (!Main.tile.TryGet(pos, out var tile) || !tile.HasUnactuatedTile || !Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] || i != 0 && tile.Slope != SlopeType.Solid) {
				continue;
			}

			// Ice can't climbed on, unless you have climbing gear
			if (OverhaulTileTags.NoClimbing.Has(tile.TileType) && !HasClimbingGear) {
				continue;
			}

			static bool CheckFree(int x, int y, Tile t)
				=> !(t.HasTile && !t.IsActuated) || !Main.tileSolid[t.TileType] || OverhaulTileTags.AllowClimbing.Has(t.TileType);

			if (!(
				TileCheckUtils.CheckAreaAll(pos.X, pos.Y - 3, 1, 3, CheckFree)
				& TileCheckUtils.CheckAreaAll(pos.X + (Player.direction == 1 ? -1 : 1), pos.Y - 3, 1, 4, CheckFree)
				& TileCheckUtils.CheckAreaAll(pos.X + (Player.direction == 1 ? -2 : 2), pos.Y - 2, 1, 3, CheckFree)
			)) {
				continue;
			}

			StartClimbing(Player.position, new Vector2(pos.X * 16f + (Player.direction == 1 ? -4f : 0f), (pos.Y - 3) * 16f  + 6));
			UpdateClimbing();
			break;
		}
	}

	private void UpdateClimbing()
	{
		var playerMovement = Player.GetModPlayer<PlayerMovement>();
		var playerRotation = Player.GetModPlayer<PlayerBodyRotation>();
		var playerAnimations = Player.GetModPlayer<PlayerAnimations>();
		var playerDirectioning = Player.GetModPlayer<PlayerDirectioning>();

		Player.gfxOffY = 0f; // Disable autostep vertical sprite offsets.

		// If we don't reset fall time, the player may explode from fall damage once they stop climbing.
		Player.fallStart = (int)(Player.position.Y / 16f);

		// Force direction.
		var climbDirection = climbStartPos.X <= climbEndPos.X ? Direction1D.Right : Direction1D.Left;

		playerDirectioning.SetDirectionOverride(climbDirection, 2, PlayerDirectioning.OverrideFlags.IgnoreItemAnimation);

		// Progress climbing.
		ClimbProgress = MathUtils.StepTowards(ClimbProgress, 1f, 1f / ClimbTime * TimeSystem.LogicDeltaTime);

		Player.position.X = MathHelper.Lerp(climbStartPos.X, climbEndPos.X, ClimbProgress);
		Player.position.Y = ClimbProgress < 0.75f ? MathHelper.Lerp(climbStartPos.Y, climbEndPos.Y, ClimbProgress / 0.75f) : climbEndPos.Y;
		playerMovement.ForcedPosition = Player.position;

		if (ClimbProgress >= 1f) {
			// End climbing

			IsClimbing = false;

			// Try to restore original velocity for a better feel
			var newVelocity = climbStartVelocity;

			newVelocity *= HasClimbingGear ? 0.9f : 0.75f;

			int xVelocityDirection = MathF.Sign(climbStartVelocity.X);
			int xKeyDirection = MathF.Sign(Player.KeyDirection().X);

			if (xVelocityDirection != (int)climbDirection) {
				newVelocity.X = 0f;
			} else if (xVelocityDirection != xKeyDirection) {
				newVelocity.X *= xKeyDirection == 0 ? 0.5f : 0f;
			} else {
				newVelocity.X += (float)climbDirection;
			}

			if (climbStartVelocity.Y > -1f) {
				newVelocity.Y = 0f;
			}

			if (Player.controlUp) {
				newVelocity.Y = Math.Min(-3f, newVelocity.Y - 2f);
			}

			Player.velocity = newVelocity;

			if (!Main.dedServ) {
				SoundEngine.PlaySound(PlayerBunnyrolls.BunnyrollSound.WithVolumeScale(0.2f), Player.Center);
			}

			return;
		}

		// Set visuals

		playerAnimations.ForcedBodyFrame = ClimbProgress > 0.75f ? PlayerFrames.Use4 : ClimbProgress > 0.5f ? PlayerFrames.Use3 : ClimbProgress > 0.25f ? PlayerFrames.Use2 : PlayerFrames.Use1;
		playerRotation.Rotation = ClimbProgress * 0.7f * Player.direction;
		playerRotation.RotationOffsetScale = 0f;

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
		Player.GetModPlayer<PlayerDodgerolls>().NoDodgerollsTimer.Set(1);
	}
}
