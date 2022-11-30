using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

/// <summary>
/// Launches players towards attack direction on use animation beginning.
/// </summary>
public sealed class ItemMeleeSwingVelocity : ItemComponent
{
	public struct ContextMatch
	{
		public bool? OnGround;
		public bool? PowerAttack;
		public Direction2D? AttackDirection;
		public Direction2D? MoveDirection;
		public Direction2D? KeyDirection;
	}

	public readonly record struct VelocityModifier(string Identifier)
	{
		public ContextMatch Predicate { get; init; } = default;
		public Vector2 VelocityMultiplier { get; init; } = default;
		public Vector2 MaxVelocityMultiplier { get; init; } = default;
	}

	public static class Modifiers
	{
		/// <summary> Boost from power attacks. </summary>
		public static readonly VelocityModifier PowerAttackBoost = new(nameof(PowerAttackBoost)) {
			Predicate = new() { PowerAttack = true },
			VelocityMultiplier = new Vector2(1.5f, 1.5f),
			MaxVelocityMultiplier = new Vector2(1.5f, 1.5f), // More stackable
		};
		
		/// <summary> Boost power attacks' vertical dashes even higher when on ground. </summary>
		public static readonly VelocityModifier PowerAttackGroundBoost = new(nameof(PowerAttackGroundBoost)) {
			Predicate = new() { PowerAttack = true, OnGround = true },
			VelocityMultiplier = new Vector2(1.35f, 1.25f),
		};

		/// <summary> Disable vertical dashes for non-charged attacks whenever the player is on ground. </summary>
		public static readonly VelocityModifier DisableVerticalDashesForNonChargedAttacks = new(nameof(DisableVerticalDashesForNonChargedAttacks)) {
			Predicate = new() { PowerAttack = false, OnGround = true },
			VelocityMultiplier = new Vector2(1.0f, 0.0f),
		};

		/// <summary>
		/// Disable upwards dashes whenever the player is falling down.
		/// Could be made more lenient.
		/// </summary>
		public static readonly VelocityModifier DisableUpwardsDashesWhenFalling = new(nameof(DisableUpwardsDashesWhenFalling)) {
			Predicate = new() { PowerAttack = false, OnGround = false, MoveDirection = Direction2D.Down, AttackDirection = Direction2D.Up },
			VelocityMultiplier = new Vector2(1.0f, 0.0f),
		};

		/// <summary>
		/// Completely disable all dashes whenever the player is staying still on the ground.
		/// </summary>
		public static readonly VelocityModifier DisableDashesForNonChargedAttacksWhenStill = new(nameof(DisableDashesForNonChargedAttacksWhenStill)) {
			Predicate = new() { PowerAttack = false, OnGround = true, MoveDirection = 0 },
			VelocityMultiplier = new Vector2(0f, 0f),
		};
	}
	
	private readonly Dictionary<string, VelocityModifier> dashVelocityModifiers = new();
	
	public Vector2 DashVelocity { get; set; } = Vector2.One;
	public Vector2 MaxDashVelocity { get; set; } = Vector2.One;
	public Vector2 DefaultKeyVelocityMultiplier { get; set; } = new Vector2(2f / 3f, 1f);

	public IReadOnlyDictionary<string, VelocityModifier> DashVelocityModifiers => dashVelocityModifiers;

	public override void UseAnimation(Item item, Player player)
	{
		if (!Enabled) {
			return;
		}

		const float AverageAnimationTime = 21f; // Comes from Platinum Broadsword.

		var dashVelocity = DashVelocity;
		int totalAnimationTime = CombinedHooks.TotalAnimationTime(item.useAnimation, player, item);
		float animationMultiplier = 1f / (AverageAnimationTime / totalAnimationTime);
		
		// TML Problem:
		// Couldn't just use MeleeAttackAiming.AttackDirection here due to TML lacking proper tools for controlling execution orders.
		// By chance, this global's hooks run before MeleeAttackAiming's.
		// -- Mirsario
		var attackDirection = player.LookDirection();

		dashVelocity *= animationMultiplier;

		// Apply data-driven modifiers

		bool powerAttack = item.GetGlobalItem<ItemPowerAttacks>().PowerAttack;
		bool onGround = player.OnGround();
		var attackDirectionEnum = attackDirection.ToDirection2D();
		var moveDirectionEnum = player.velocity.ToDirection2D();
		var keyDirection = player.KeyDirection();
		var keyDirectionEnum = keyDirection.ToDirection2D();
		
		static bool BoolCheck(bool? checkedValue, bool baseValue)
			=> !checkedValue.HasValue || checkedValue.Value == baseValue;

		static bool DirectionCheck(Direction2D? checkedValue, Direction2D baseValue)
			=> !checkedValue.HasValue || (baseValue & checkedValue.Value) == checkedValue.Value;

		var maxDashVelocityMultiplier = Vector2.One;

		foreach (var modifier in DashVelocityModifiers.Values) {
			var predicate = modifier.Predicate;
			
			if (!BoolCheck(predicate.OnGround, onGround)
			|| !BoolCheck(predicate.PowerAttack, powerAttack)
			|| !DirectionCheck(predicate.AttackDirection, attackDirectionEnum)
			|| !DirectionCheck(predicate.MoveDirection, moveDirectionEnum)
			|| !DirectionCheck(predicate.KeyDirection, keyDirectionEnum)) {
				continue;
			}

			dashVelocity *= modifier.VelocityMultiplier;
			maxDashVelocityMultiplier *= modifier.MaxVelocityMultiplier;
		}

		// Calculate max velocity

		var maxDashVelocity = MaxDashVelocity * maxDashVelocityMultiplier;

		maxDashVelocity = new Vector2(
			maxDashVelocity.X == 0f ? Math.Max(maxDashVelocity.X, Math.Abs(dashVelocity.X)) : maxDashVelocity.X,
			maxDashVelocity.Y == 0f ? Math.Max(maxDashVelocity.Y, Math.Abs(dashVelocity.Y)) : maxDashVelocity.Y
		);

		// Multiply by controls

		const float ZeroedAreaFactor = 0.25f;

		float? CalculateControlMultiplier(int axis, float keyAxisValue)
		{
			if (axis != 0 && axis != 1) {
				throw new ArgumentOutOfRangeException(nameof(axis));
			}

			if (keyAxisValue == 0f) {
				return null;
			}

			float otherAxis = 1f - Math.Abs(keyAxisValue);
			Vector2 adjustedKeyDirection = axis == 0
				? new Vector2(keyAxisValue, otherAxis)
				: new Vector2(otherAxis, keyAxisValue);

			adjustedKeyDirection = Vector2.Normalize(adjustedKeyDirection);

			float dotProduct = Vector2.Dot(attackDirection, adjustedKeyDirection);
			float controlMultiplier = (dotProduct + 1f) * 0.5f;

			controlMultiplier = (controlMultiplier - ZeroedAreaFactor) / (1f - ZeroedAreaFactor);
			controlMultiplier = MathHelper.Clamp(controlMultiplier, 0f, 1f);

			return controlMultiplier;
		}

		Vector2 controlMultipliers;
		
		controlMultipliers.X = CalculateControlMultiplier(0, keyDirection.X) ?? DefaultKeyVelocityMultiplier.X;
		controlMultipliers.Y = CalculateControlMultiplier(1, keyDirection.Y) ?? DefaultKeyVelocityMultiplier.Y;

		dashVelocity *= controlMultipliers;

		// Add the velocity

		player.AddLimitedVelocity(dashVelocity * attackDirection, maxDashVelocity);
	}
	
	public void AddVelocityModifier(in VelocityModifier modifier)
		=> dashVelocityModifiers.Add(modifier.Identifier, modifier);
	
	public void RemoveVelocityModifier(in VelocityModifier modifier)
		=> dashVelocityModifiers.Remove(modifier.Identifier);

	public void RemoveVelocityModifier(string name)
		=> dashVelocityModifiers.Remove(name);
}
