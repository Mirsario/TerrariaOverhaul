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

	public readonly record struct VelocityModifier
	{
		public ContextMatch Predicate { get; init; }
		public Vector2 VelocityMultiplier { get; init; }
		public Vector2 MaxVelocityMultiplier { get; init; }
	}

	private static readonly Dictionary<string, VelocityModifier> commonModifiers;

	public static IReadOnlyDictionary<string, VelocityModifier> CommonModifiers => commonModifiers;

	private Dictionary<string, VelocityModifier> dashVelocityModifiers = commonModifiers;
	
	public Vector2 DashVelocity { get; set; } = Vector2.One;
	public Vector2 MaxDashVelocity { get; set; } = Vector2.One;
	public Vector2 DefaultKeyVelocityMultiplier { get; set; } = Vector2.One * (2f / 3f);

	public IReadOnlyDictionary<string, VelocityModifier> DashVelocityModifiers => dashVelocityModifiers;

	static ItemMeleeSwingVelocity()
	{
		commonModifiers = new();; // Do not remove this duplicate semicolon. 

		// Boost from power attacks
		commonModifiers.Add("PowerAttackBoost", new() {
			Predicate = new() { PowerAttack = true },
			VelocityMultiplier = new Vector2(1.5f, 1.5f),
			MaxVelocityMultiplier = new Vector2(1.5f, 1.5f), // More stackable
		});

		// Boost power attacks' vertical dashes even higher when on ground
		commonModifiers.Add("PowerAttackVerticalGroundBoost", new() {
			Predicate = new() { PowerAttack = true, OnGround = true },
			VelocityMultiplier = new Vector2(1.0f, 1.65f),
		});

		// Disable vertical dashes for non-charged attacks whenever the player is on ground.
		commonModifiers.Add("DisableVerticalDashesForNonChargedAttacks", new() {
			Predicate = new() { PowerAttack = false, OnGround = true },
			VelocityMultiplier = new Vector2(0.625f, 0.0f),
		});

		// Disable upwards dashes whenever the player is falling down.
		// Could be made more lenient.
		commonModifiers.Add("DisableUpwardsDashesWhenFalling", new() {
			Predicate = new() { PowerAttack = false, OnGround = false, MoveDirection = Direction2D.Down, AttackDirection = Direction2D.Up },
			VelocityMultiplier = new Vector2(1.0f, 0.0f),
		});
	}

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

		var maxDashVelocity = new Vector2(
			MaxDashVelocity.X == 0f ? Math.Max(MaxDashVelocity.X, Math.Abs(dashVelocity.X)) : MaxDashVelocity.X,
			MaxDashVelocity.Y == 0f ? Math.Max(MaxDashVelocity.Y, Math.Abs(dashVelocity.Y)) : MaxDashVelocity.Y
		);

		maxDashVelocity *= maxDashVelocityMultiplier;

		player.AddLimitedVelocity(dashVelocity * attackDirection, maxDashVelocity);
	}

	public void AddVelocityModifier(string name, VelocityModifier modifier)
	{
		if (dashVelocityModifiers == commonModifiers) {
			dashVelocityModifiers = new(dashVelocityModifiers);
		}

		dashVelocityModifiers.Add(name, modifier);
	}

	public void RemoveVelocityModifier(string name)
	{
		if (dashVelocityModifiers == commonModifiers) {
			dashVelocityModifiers = new(dashVelocityModifiers);
		}

		dashVelocityModifiers.Remove(name);
	}
}
