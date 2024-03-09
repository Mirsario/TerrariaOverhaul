using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Damage;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Content.Buffs;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

#pragma warning disable IDE0060 // Remove unused parameter

namespace TerrariaOverhaul.Common.Dodgerolls;

public struct DodgerollStats
{
	public uint MaxCharges = 2;
	// Timings
	public uint CooldownLength = 90;
	public uint DodgerollLength = 22;
	public uint BufferingLength = 20;
	public uint MinItemUseCommitment = 20;
	public uint CounterBuffLength = 90;
	// Velocity
	public float AirSpeed = 3.90f;
	public float GroundSpeed = 6.00f;
	public float VelocityApplicationPeriod = 0.5f;
	// Fall Damage Reduction
	public float FallDamageReductionPeriod = 0.67f;
	public float FallDamageReductionMultiplier = 0.35f;
	// Damage done via direct collision
	public int DirectDamage = 0;
	public float DirectKnockback = 0f;
	// Damage done via opening doors
	public int TransferredDamage = 10;
	public float TransferredKnockback = 8.0f;

	public DodgerollStats() { }
}

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
	public static DodgerollStats DefaultStats { get; set; } = new();


	public DodgerollStats Stats = DefaultStats;
	public Timer TirednessTimer;
	public Timer NoDodgerollsTimer;
	public Timer DodgeAttemptTimer;
	public bool ForceDodgeroll;
	public Direction1D WantedDirection;

	public uint CurrentCharges { get; set; }
	public bool IsDodging { get; private set; }
	public uint DodgeTime { get; private set; }
	public float StartRotation { get; private set; }
	public float StartItemRotation { get; private set; }
	public Vector2 StartVelocity { get; private set; }
	public Direction1D DodgeDirection { get; private set; }
	public Direction1D DodgeDirectionVisual { get; private set; }

	public override void Load()
	{
		// Make GUI-related sounds ignored by reverb & other filters.
		AudioEffectsSystem.SetEnabledForSoundStyle(FailureSound, false);
		AudioEffectsSystem.SetEnabledForSoundStyle(RechargedSound, false);

		DodgerollKey = KeybindLoader.RegisterKeybind(Mod, "Dodgeroll", Keys.LeftControl);

		IL_Player.Update_NPCCollision += PlayerNpcCollisionInjection;
		IL_Projectile.Damage += ProjectileDamageInjection;
	}

	public override void Initialize()
	{
		ResetEffects();

		CurrentCharges = Stats.MaxCharges;
	}

	public override void ResetEffects()
	{
		Stats = DefaultStats;
	}

	public override bool PreItemCheck()
	{
		CurrentCharges = Math.Min(CurrentCharges, Stats.MaxCharges);

		UpdateCooldowns();
		UpdateDodging();

		// Stop umbrella and other things from working
		if (IsDodging && Player.HeldItem.type == ItemID.Umbrella) {
			return false;
		}

		return true;
	}

	public override bool CanUseItem(Item item)
	{
		// Disallow item use during a dodgeroll;
		if (IsDodging) {
			return false;
		}

		// And also when one is enqueued, so that autoReuse doesn't trigger.
		if (DodgeAttemptTimer.Active) {
			return false;
		}

		return true;
	}

	public void QueueDodgeroll(uint minAttemptTimer, Direction1D direction, bool force = false)
	{
		if (force) {
			CurrentCharges = Math.Max(CurrentCharges, 1);
		} else if (CurrentCharges == 0) {
			if (!Main.dedServ && Player.IsLocal()) {
				SoundEngine.PlaySound(FailureSound);
			}

			return;
		}

		DodgeAttemptTimer.Set(minAttemptTimer);

		WantedDirection = direction;
	}

	private void UpdateCooldowns()
	{
		if (!TirednessTimer.Active && CurrentCharges < Stats.MaxCharges) {
			var soundStyle = RechargedSound;

			if (TirednessTimer.Length > Stats.CooldownLength) {
				CurrentCharges = Stats.MaxCharges;

				soundStyle.Pitch = 0.1f;
				soundStyle.Volume = 1.0f;
			} else {
				CurrentCharges += 1;
				TirednessTimer.Set(Stats.CooldownLength);

				soundStyle.PitchVariance /= 3f;
				soundStyle.Pitch = MathHelper.Lerp(-0.5f, 0.0f, CurrentCharges / (float)Stats.MaxCharges);
			}

			if (!Main.dedServ && Player.IsLocal()) {
				SoundEngine.PlaySound(soundStyle);
			}
		}
	}

	private bool TryStartDodgeroll()
	{
		bool isLocal = Player.IsLocal();

		if (isLocal && !DodgeAttemptTimer.Active && DodgerollKey.JustPressed && (!Player.mouseInterface || !Main.playerInventory)) {
			int keyDirection = (int)Player.KeyDirection().X;
			Direction1D chosenDirection = keyDirection switch {
				1 => Direction1D.Right,
				-1 => Direction1D.Left,
				_ => (Direction1D)Player.direction,
			};

			QueueDodgeroll(Stats.BufferingLength, chosenDirection);
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
			if (Player.mount != null && Player.mount.Active) {
				return false;
			}

			// Handle item use
			if (Player.ItemAnimationActive && Player.HeldItem is Item heldItem) {
				uint timeSinceItemUseStart = !Player.TryGetModPlayer(out PlayerItemUse playerItemUse)
					? (uint)Math.Max(0, Player.itemAnimationMax - Player.itemAnimation)
					: playerItemUse.TimeSinceLastUseAnimation;

				// Enforce a minimal commitment timeframe.
				if (timeSinceItemUseStart < Stats.MinItemUseCommitment) {
					return false;
				}

				// If the item has a reuseDelay - Deny the roll up until it's activated.
				// (Player.reuseDelay works by getting swapped into itemAnim once itemAnim first reaches zero) 
				if (heldItem.reuseDelay > 0 && Player.reuseDelay != 0) {
					return false;
				}

				// Handle melee in a special way, ensuring that rolling during a slash is not possible.
				if (!heldItem.noMelee && ICanDoMeleeDamage.Invoke(heldItem, Player)) {
					return false;
				}
			}
		}

		DodgeAttemptTimer = 0;

		/*if (onFire) {
			// Don't stop but roll
			int fireBuff = player.FindBuffIndex(24);
			int fireBuff2 = player.FindBuffIndex(39);
		
			if (fireBuff != -1) {
				player.buffTime[fireBuff] -= 90;
			}
		
			if (fireBuff2 != -1) {
				player.buffTime[fireBuff2] -= 90;
			}
		}*/

		if (!Main.dedServ) {
			SoundEngine.PlaySound(DodgerollSound, Player.Center);
		}

		Player.StopGrappling();

		Player.channel = false;
		Player.eocHit = 1;

		IsDodging = true;
		DodgeTime = 0;
		StartVelocity = Player.velocity;
		StartRotation = Player.GetModPlayer<PlayerBodyRotation>().Rotation;
		StartItemRotation = Player.itemRotation;
		DodgeDirectionVisual = (Direction1D)Player.direction;
		DodgeDirection = WantedDirection != 0 ? WantedDirection : (Direction1D)Player.direction;

		// Handle cooldowns

		CurrentCharges = Math.Max(0, CurrentCharges - 1);

		// Activate tiredness, which doesn't stop the next dodgeroll on its own
		uint tirednessTime = Stats.CooldownLength;

		if (CurrentCharges == 0 && Stats.MaxCharges > 1) {
			tirednessTime += Stats.CooldownLength * (Stats.MaxCharges - 1);
		}

		TirednessTimer.Set(tirednessTime);

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

		float dodgeProgress = DodgeTime / (float)Stats.DodgerollLength;

		// Lower fall damage
		if (onGround && !wasOnGround && dodgeProgress < Stats.FallDamageReductionPeriod) {
			int tilePositionY = (int)(Player.position.Y / 16f);

			Player.fallStart = (int)MathHelper.Lerp(Player.fallStart, tilePositionY, Stats.FallDamageReductionMultiplier);
		}

		// Open doors
		TryOpeningDoors();

		// Apply velocity
		float lesserSpeed = MathF.Min(Stats.GroundSpeed, Stats.AirSpeed);
		float greaterSpeed = MathF.Max(Stats.GroundSpeed, Stats.AirSpeed);
		float speedTypeDiff = greaterSpeed - lesserSpeed;

		if (dodgeProgress <= Stats.VelocityApplicationPeriod) {
			float newVelX = (onGround ? Stats.GroundSpeed : Stats.AirSpeed) * (sbyte)DodgeDirection;

			if (Math.Abs(Player.velocity.X) < Math.Abs(newVelX) || Math.Sign(newVelX) != Math.Sign(Player.velocity.X)) {
				Player.velocity.X = newVelX;
			}
		}

		if (!onGround && wasOnGround && MathF.Abs(Player.velocity.X) >= lesserSpeed) {
			int sign = Math.Sign(Player.velocity.X);
			float newVelX = MathUtils.MaxAbs(
				Player.velocity.X - (speedTypeDiff * sign),
				lesserSpeed * sign
			);

			if (Math.Sign(newVelX) == Math.Sign(StartVelocity.X)) {
				newVelX = MathUtils.MaxAbs(newVelX, StartVelocity.X);
			}

			Player.velocity.X = newVelX;
		}

		if (!Main.dedServ) {
			// Trail
			Player.GetModPlayer<PlayerTrailEffects>().ForceTrailEffect(2);
		}

		Player.pulley = false;

		// Apply rotations & direction
		Player.GetModPlayer<PlayerItemRotation>().ForcedItemRotation = StartItemRotation;
		Player.GetModPlayer<PlayerAnimations>().ForcedLegFrame = PlayerFrames.Jump;
		Player.GetModPlayer<PlayerDirectioning>().SetDirectionOverride(DodgeDirectionVisual, 2, PlayerDirectioning.OverrideFlags.IgnoreItemAnimation);

		rotation = DodgeDirection == Direction1D.Right
			? Math.Min(+MathHelper.TwoPi, MathHelper.Lerp(StartRotation, +MathHelper.TwoPi, dodgeProgress))
			: Math.Max(-MathHelper.TwoPi, MathHelper.Lerp(StartRotation, -MathHelper.TwoPi, dodgeProgress));

		// Progress the dodgeroll
		DodgeTime++;

		// Prevent other actions
		Player.GetModPlayer<PlayerClimbing>().ClimbCooldown.Set(1);

		if (DodgeTime >= Stats.DodgerollLength) {
			IsDodging = false;
			Player.eocDash = 0;
			//forceSyncControls = true;
		} else {
			Player.runAcceleration = 0f;
		}
	}

	private bool TryOpeningDoors()
	{
		const int DoorWidth = 2;
		const int DoorHeight = 3;

		var tilePos = Player.position.ToTileCoordinates16();
		int x = DodgeDirection > 0 ? tilePos.X + DoorWidth : tilePos.X - 1;

		for (int y = tilePos.Y; y < tilePos.Y + DoorHeight; y++) {
			if (!Main.tile.TryGet(x, y, out var tile)) {
				continue;
			}

			if (tile.TileType != TileID.ClosedDoor) {
				continue;
			}

			if (WorldGen.OpenDoor(x, y, (int)DodgeDirection)) {
				if (Player.IsLocal()) {
					var damageRect = new Rectangle(x * 16, y * 16 - 8, 48, 64);

					if (DodgeDirection == Direction1D.Left) {
						damageRect.X -= DoorWidth * 16;
					}

					foreach (var npc in ActiveEntities.NPCs) {
						if (npc.GetRectangle().Intersects(damageRect)) {
							Player.ApplyDamageToNPC(npc, Stats.TransferredDamage, Stats.TransferredKnockback, (int)DodgeDirection, crit: true);

							if (!npc.boss && !NPCID.Sets.ShouldBeCountedAsBoss[npc.type] && npc.TryGetGlobalNPC(out NPCAttackCooldowns cooldowns)) {
								cooldowns.SetAttackCooldown(npc, 60, true);
							}
						}
					}
				}

				return true;
			}
		}

		return false;
	}

	private void OnDodgeEntity(Player player, Entity entity)
	{
		if (!Main.dedServ && !player.HasBuff<CriticalJudgement>()) {
			/*
			SoundEngine.PlaySound(new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Anime") {
				Pitch = 0.10f,
				Volume = 0.20f,
			});
			*/
		}

		player.AddBuff(ModContent.BuffType<CriticalJudgement>(), (int)Stats.CounterBuffLength);
	}

	private static bool LateCanBeHitByEntity(Player player, Entity entity)
	{
		if (player.TryGetModPlayer(out PlayerDodgerolls dodgerolls) && dodgerolls.IsDodging) {
			dodgerolls.OnDodgeEntity(player, entity);

			return false;
		}

		return true;
	}

	private static void PlayerNpcCollisionInjection(ILContext context)
	{
		var il = new ILCursor(context);
		bool debugAssembly = OverhaulMod.TMLAssembly.IsDebugAssembly();

		ILLabel? continueLabel = null;
		int npcIndexLocalId = -1;

		// Go to the bottom.
		il.Index = context.Instrs.Count - 1;

		// Match the last 'if (npcTypeNoAggro[Main.npc[i].type])'.
		if (!debugAssembly) {
			il.GotoPrev(
				MoveType.After,
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Player), nameof(Terraria.Player.npcTypeNoAggro)),
				i => i.MatchLdsfld(typeof(Main), nameof(Main.npc)),
				i => i.MatchLdloc(out npcIndexLocalId),
				i => i.MatchLdelemRef(),
				i => i.MatchLdfld(typeof(NPC), nameof(NPC.type)),
				i => i.MatchLdelemU1(),
				i => i.MatchBrtrue(out continueLabel)
			);
		} else {
			il.GotoPrev(
				MoveType.After,
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Player), nameof(Terraria.Player.npcTypeNoAggro)),
				i => i.MatchLdsfld(typeof(Main), nameof(Main.npc)),
				i => i.MatchLdloc(out npcIndexLocalId),
				i => i.MatchLdelemRef(),
				i => i.MatchLdfld(typeof(NPC), nameof(NPC.type)),
				i => i.MatchLdelemU1(),
				i => i.MatchStloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchBrfalse(out _),
				i => i.MatchBr(out continueLabel)
			);
		}

		il.HijackIncomingLabels();

		il.Emit(OpCodes.Ldarg_0);
		il.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.npc))!);
		il.Emit(OpCodes.Ldloc, npcIndexLocalId);
		il.Emit(OpCodes.Ldelem_Ref);
		il.EmitDelegate((Player player, NPC npc) => LateCanBeHitByEntity(player, npc));
		il.Emit(OpCodes.Brfalse, continueLabel!);
	}

	private static void ProjectileDamageInjection(ILContext context)
	{
		var il = new ILCursor(context);
		bool debugAssembly = OverhaulMod.TMLAssembly.IsDebugAssembly();

		ILLabel? skipHitLabel = null;

		// Match the last 'if (!Main.player[myPlayer2].CanParryAgainst(Main.player[myPlayer2].Hitbox, base.Hitbox, velocity))'.
		il.GotoNext(
			MoveType.After,
			i => i.MatchCallvirt(typeof(Player), nameof(Player.CanParryAgainst))
		);

		if (!debugAssembly) {
			il.GotoNext(
				MoveType.After,
				i => i.MatchBrtrue(out skipHitLabel)
			);
		} else {
			il.GotoNext(
				MoveType.After,
				i => i.MatchBrfalse(out skipHitLabel)
			);
		}

		il.HijackIncomingLabels();

		int emitLocation = il.Index;

		// Find player local
		int playerIndexLocalId = -1;

		il.GotoPrev(
			i => i.MatchLdsfld(typeof(Main), nameof(Main.player)),
			i => i.MatchLdloc(out playerIndexLocalId),
			i => i.MatchLdelemRef()
		);

		// Go back and emit

		il.Index = emitLocation;

		il.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.player))!);
		il.Emit(OpCodes.Ldloc, playerIndexLocalId);
		il.Emit(OpCodes.Ldelem_Ref);
		il.Emit(OpCodes.Ldarg_0);
		il.EmitDelegate((Player player, Projectile projectile) => LateCanBeHitByEntity(player, projectile));
		il.Emit(OpCodes.Brfalse, skipHitLabel!);
	}
}
