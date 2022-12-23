using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Common.PlayerEffects;
using TerrariaOverhaul.Content.Buffs;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Dodgerolls;

public sealed class PlayerDodgerolls : ModPlayer
{
	public static readonly SoundStyle DodgerollSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/Armor", 3) {
		Volume = 0.65f,
		PitchVariance = 0.2f
	};
	public static readonly SoundStyle FailureSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/NoAmmo") {
		Volume = 0.2f,
		Pitch = 0.5f,
		PitchVariance = 0.2f
	};
	public static readonly SoundStyle RechargedSound = new($"{nameof(TerrariaOverhaul)}/Common/Dodgerolls/DodgerollReady", 2) {
		Volume = 0.9f,
		PitchVariance = 0.125f
	};

	public static ModKeybind DodgerollKey { get; private set; } = null!;

	public static float DodgeTimeMax => 0.37f;
	public static uint DefaultDodgeTirednessTime => (uint)(TimeSystem.LogicFramerate * 1.5f);
	public static uint DefaultDodgeCooldownTime => DefaultDodgeTirednessTime * 2;
	public static int DefaultDodgeMaxCharges => 2;

	public Timer DodgerollTirednessTimer;
	public Timer DodgerollCooldownTimer;
	public Timer NoDodgerollsTimer;
	public Timer DodgeAttemptTimer;
	public bool ForceDodgeroll;
	public sbyte WantedDodgerollDirection;

	public int MaxCharges { get; set; }
	public int CurrentCharges { get; set; }

	public bool IsDodging { get; private set; }
	public float DodgeStartRotation { get; private set; }
	public float DodgeItemRotation { get; private set; }
	public float DodgeTime { get; private set; }
	public sbyte DodgeDirection { get; private set; }
	public sbyte DodgeDirectionVisual { get; private set; }

	public override void Load()
	{
		// Make GUI-related sounds ignored by reverb & other filters.
		AudioEffectsSystem.IgnoreSoundStyle(FailureSound);
		AudioEffectsSystem.IgnoreSoundStyle(RechargedSound);

		DodgerollKey = KeybindLoader.RegisterKeybind(Mod, "Dodgeroll", Keys.LeftControl);
	}

	public override void Initialize()
	{
		CurrentCharges = MaxCharges = DefaultDodgeMaxCharges;
	}

	public override bool PreItemCheck()
	{
		UpdateCooldowns();
		UpdateDodging();

		// Stop umbrella and other things from working
		if (IsDodging && Player.HeldItem.type == ItemID.Umbrella) {
			return false;
		}

		return true;
	}

	// CanX
	public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
		=> !IsDodging;

	public override bool CanBeHitByProjectile(Projectile proj)
		=> !IsDodging;

	public override bool CanUseItem(Item item)
		=> !IsDodging;

	public void QueueDodgeroll(uint minAttemptTimer, sbyte direction, bool force = false)
	{
		if (force) {
			DodgerollCooldownTimer = 0;
			CurrentCharges = Math.Max(CurrentCharges, 1);
		} else if (CurrentCharges == 0) {
			if (!Main.dedServ && Player.IsLocal()) {
				SoundEngine.PlaySound(FailureSound);
			}

			return;
		}

		DodgeAttemptTimer.Set(minAttemptTimer);

		WantedDodgerollDirection = direction;
	}

	private void UpdateCooldowns()
	{
		if (!DodgerollTirednessTimer.Active && CurrentCharges < MaxCharges) {
			CurrentCharges = MaxCharges;

			if (!Main.dedServ && Player.IsLocal()) {
				SoundEngine.PlaySound(RechargedSound);
			}
		}
	}

	private bool TryStartDodgeroll()
	{
		bool isLocal = Player.IsLocal();

		if (isLocal && !DodgeAttemptTimer.Active && DodgerollKey.JustPressed && !Player.mouseInterface) {
			QueueDodgeroll((uint)(TimeSystem.LogicFramerate * 0.333f), (sbyte)Math.Sign(Player.KeyDirection().X));
		}

		if (!ForceDodgeroll) {
			// Only initiate dodgerolls locally.
			if (!isLocal) {
				return false;
			}

			// Input & cooldown check. The cooldown can be enforced by other actions.
			if (!DodgeAttemptTimer.Active || NoDodgerollsTimer.Active || CurrentCharges == 0) {
				return false;
			}

			// Don't allow dodging on mounts and during item use.
			if (Player.mount != null && Player.mount.Active || Player.itemAnimation > 0) {
				return false;
			}
		}

		DodgeAttemptTimer = 0;

		/*if(onFire) {
			// Don't stop but roll
			int fireBuff = player.FindBuffIndex(24);
			int fireBuff2 = player.FindBuffIndex(39);
		
			if(fireBuff!=-1) {
				player.buffTime[fireBuff] -= 90;
			}
		
			if(fireBuff2!=-1) {
				player.buffTime[fireBuff2] -= 90;
			}
		}*/

		if (!Main.dedServ) {
			SoundEngine.PlaySound(DodgerollSound, Player.Center);
		}

		Player.StopGrappling();

		Player.eocHit = 1;

		IsDodging = true;
		DodgeStartRotation = Player.GetModPlayer<PlayerBodyRotation>().Rotation;
		DodgeItemRotation = Player.itemRotation;
		DodgeTime = 0f;
		DodgeDirectionVisual = (sbyte)Player.direction;
		DodgeDirection = WantedDodgerollDirection != 0 ? WantedDodgerollDirection : (sbyte)Player.direction;

		// Handle cooldowns

		CurrentCharges = Math.Max(0, CurrentCharges - 1);

		// Activate tiredness, which doesn't stop the next dodgeroll on its own
		uint tirednessTime = CurrentCharges == 0 ? DefaultDodgeCooldownTime : DefaultDodgeTirednessTime;

		DodgerollTirednessTimer.Set(tirednessTime);

		if (!isLocal) {
			ForceDodgeroll = false;
		} else if (Main.netMode != NetmodeID.SinglePlayer) {
			MultiplayerSystem.SendPacket(new PlayerDodgerollPacket(Player));
		}

		return true;
	}

	private void UpdateDodging()
	{
		if (Player.mount.Active) {
			IsDodging = false;

			return;
		}

		bool onGround = Player.OnGround();
		bool wasOnGround = Player.WasOnGround();

		ref float rotation = ref Player.GetModPlayer<PlayerBodyRotation>().Rotation;

		// Attempt to initiate a dodgeroll if the player isn't doing one already.
		if (!IsDodging && !TryStartDodgeroll()) {
			return;
		}

		// Lower fall damage
		if (DodgeTime < DodgeTimeMax / 1.5f && onGround && !wasOnGround) {
			Player.fallStart = (int)MathHelper.Lerp(Player.fallStart, (int)(Player.position.Y / 16f), 0.35f);
		}

		// Open doors
		var tilePos = Player.position.ToTileCoordinates16();
		int x = DodgeDirection > 0 ? tilePos.X + 2 : tilePos.X - 1;

		for (int y = tilePos.Y; y < tilePos.Y + 3; y++) {
			if (!Main.tile.TryGet(x, y, out var tile)) {
				continue;
			}

			if (tile.TileType == TileID.ClosedDoor) {
				WorldGen.OpenDoor(x, y, DodgeDirection);
			}
		}

		// Apply velocity
		if (DodgeTime < DodgeTimeMax * 0.5f) {
			float newVelX = (onGround ? 6f : 4f) * DodgeDirection;

			if (Math.Abs(Player.velocity.X) < Math.Abs(newVelX) || Math.Sign(newVelX) != Math.Sign(Player.velocity.X)) {
				Player.velocity.X = newVelX;
			}
		}

		if (!Main.dedServ) {
			// Trail
			Player.GetModPlayer<PlayerTrailEffects>().ForceTrailEffect(2);
		}

		Player.pulley = false;

		// Apply rotations & direction
		Player.GetModPlayer<PlayerItemRotation>().ForcedItemRotation = DodgeItemRotation;
		Player.GetModPlayer<PlayerAnimations>().ForcedLegFrame = PlayerFrames.Jump;
		Player.GetModPlayer<PlayerDirectioning>().ForcedDirection = DodgeDirectionVisual;

		rotation = DodgeDirection == 1
			? Math.Min(MathHelper.Pi * 2f, MathHelper.Lerp(DodgeStartRotation, MathHelper.TwoPi, DodgeTime / (DodgeTimeMax * 1f)))
			: Math.Max(-MathHelper.Pi * 2f, MathHelper.Lerp(DodgeStartRotation, -MathHelper.TwoPi, DodgeTime / (DodgeTimeMax * 1f)));

		// Progress the dodgeroll
		DodgeTime += 1f / 60f;

		// Prevent other actions
		Player.GetModPlayer<PlayerClimbing>().ClimbCooldown.Set(1);

		if (DodgeTime >= DodgeTimeMax) {
			IsDodging = false;
			Player.eocDash = 0;

			//forceSyncControls = true;
		} else {
			Player.runAcceleration = 0f;
		}
	}
}
