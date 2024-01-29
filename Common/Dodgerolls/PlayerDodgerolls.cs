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
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Content.Buffs;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

#pragma warning disable IDE0060 // Remove unused parameter

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
	public Direction1D WantedDodgerollDirection;

	public int MaxCharges { get; set; }
	public int CurrentCharges { get; set; }
	public int DoorRollDamage { get; set; } = 10;
	public float DoorRollKnockback { get; set; } = 8.00f;

	public bool IsDodging { get; private set; }
	public float DodgeStartRotation { get; private set; }
	public float DodgeItemRotation { get; private set; }
	public float DodgeTime { get; private set; }
	public Direction1D DodgeDirection { get; private set; }
	public Direction1D DodgeDirectionVisual { get; private set; }

	public override void Load()
	{
		// Make GUI-related sounds ignored by reverb & other filters.
		AudioEffectsSystem.IgnoreSoundStyle(FailureSound);
		AudioEffectsSystem.IgnoreSoundStyle(RechargedSound);

		DodgerollKey = KeybindLoader.RegisterKeybind(Mod, "Dodgeroll", Keys.LeftControl);

		IL_Player.Update_NPCCollision += PlayerNpcCollisionInjection;
		IL_Projectile.Damage += ProjectileDamageInjection;
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

		if (isLocal && !DodgeAttemptTimer.Active && DodgerollKey.JustPressed && (!Player.mouseInterface || !Main.playerInventory)) {
			int keyDirection = (int)Player.KeyDirection().X;
			Direction1D chosenDirection = keyDirection switch {
				1 => Direction1D.Right,
				-1 => Direction1D.Left,
				_ => (Direction1D)Player.direction,
			};

			QueueDodgeroll((uint)(TimeSystem.LogicFramerate * 0.333f), chosenDirection);
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
		}

		if (Player.ItemAnimationActive) {
			Player.channel = false;
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
		DodgeDirectionVisual = (Direction1D)Player.direction;
		DodgeDirection = WantedDodgerollDirection != 0 ? WantedDodgerollDirection : (Direction1D)Player.direction;

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
		TryOpeningDoors();

		// Apply velocity
		if (DodgeTime < DodgeTimeMax * 0.5f) {
			float newVelX = (onGround ? 6f : 4f) * (sbyte)DodgeDirection;

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
		Player.GetModPlayer<PlayerDirectioning>().SetDirectionOverride(DodgeDirectionVisual, 2, PlayerDirectioning.OverrideFlags.IgnoreItemAnimation);

		rotation = DodgeDirection == Direction1D.Right
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

	private bool TryOpeningDoors()
	{
		var tilePos = Player.position.ToTileCoordinates16();
		int x = DodgeDirection > 0 ? tilePos.X + 2 : tilePos.X - 1;

		for (int y = tilePos.Y; y < tilePos.Y + 3; y++) {
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
						damageRect.X -= 32;
					}

					foreach (var npc in ActiveEntities.NPCs) {
						if (npc.GetRectangle().Intersects(damageRect)) {
							Player.ApplyDamageToNPC(npc, DoorRollDamage, DoorRollKnockback, (int)DodgeDirection, crit: true);

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

		player.AddBuff(ModContent.BuffType<CriticalJudgement>(), 90);
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
