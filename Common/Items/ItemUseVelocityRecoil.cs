using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Items;

public sealed class ItemUseVelocityRecoil : ItemComponent
{
	public Vector2 BaseVelocity { get; set; } = new(5.0f, 5.0f);
	public Vector2 MaxVelocity { get; set; } = new(5f, 5f);

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

		// Disable horizontal velocity recoil whenever the player is holding a directional key opposite to the direction of the dash.
		if (Math.Sign(player.KeyDirection().X) == -Math.Sign(velocity.X)) {
			velocity.X = 0f;
		}

		// Disable vertical velocity whenever aiming upwards or standing on the ground
		if (velocity.Y > 0f || player.velocity.Y == 0f) {
			velocity.Y = 0f;
		}

		VelocityUtils.AddLimitedVelocity(player, velocity, MaxVelocity);

		return base.UseItem(item, player);
	}
}
