// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Items;

public sealed class ItemUseVelocityRecoil : ItemComponent
{
	public Vector2 BaseVelocity { get; set; } = new(5.0f, 5.0f);
	public Vector2 MaxVelocity { get; set; } = new(5f, 5f);
	public (Vector2 Min, Vector2 Max) GravityFactor { get; set; } = (Vector2.One * 0.1f, Vector2.One);

	public override bool? UseItem(Item item, Player player)
	{
		if (!Enabled) {
			return base.UseItem(item, player);
		}

		var mouseWorld = player.GetModPlayer<PlayerDirectioning>().MouseWorld;
		var direction = (player.Center - mouseWorld).SafeNormalize(default);
		var modifiedDirection = new Vector2(direction.X, direction.Y * Math.Abs(direction.Y));

		float useTimeInSeconds = item.useTime * TimeSystem.LogicDeltaTime;

		var velocity = modifiedDirection * BaseVelocity * useTimeInSeconds;

		if (item.TryGetGlobalItem(out ItemPowerAttacks powerAttacks) && powerAttacks.Charge is { UnclampedUnfrozenValue: >= -1 } charge) {
			velocity *= charge.Progress;
		}

		// Disable horizontal velocity recoil whenever the player is holding a directional key opposite to the direction of the dash.
		if (Math.Sign(player.KeyDirection().X) == -Math.Sign(velocity.X)) {
			velocity.X = 0f;
		}

		// Disable vertical velocity whenever aiming upwards or standing on the ground
		if (velocity.Y > 0f || player.velocity.Y == 0f) {
			velocity.Y = 0f;
		}

		// Multiply by gravity, to prevent insane travel with featherfall potions.
		float gravityFactor = player.gravity / Player.defaultGravity;
		velocity *= Vector2.Clamp(Vector2.One * gravityFactor, GravityFactor.Min, GravityFactor.Max);

		VelocityUtils.AddLimitedVelocity(player, velocity, MaxVelocity);

		return base.UseItem(item, player);
	}
}
