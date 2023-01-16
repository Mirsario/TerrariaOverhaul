﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Utilities;

public static partial class PlayerExtensions
{
	// Essentials

	public static bool IsLocal(this Player player)
		=> player.whoAmI == Main.myPlayer;

	public static bool OnGround(this Player player)
		=> player.velocity.Y == 0f; //player.GetModPlayer<PlayerMovement>().OnGround;

	public static bool WasOnGround(this Player player)
		=> player.oldVelocity.Y == 0f; //player.GetModPlayer<PlayerMovement>().WasOnGround;

	public static bool IsUnderwater(this Player player)
		=> Collision.DrownCollision(player.position, player.width, player.height, player.gravDir);

	public static Vector2 KeyDirection(this Player player)
	{
		Vector2 result;
		
		result.X = (player.controlRight ? 1f : 0f) - (player.controlLeft ? 1f : 0f);
		result.Y = (player.controlDown ? 1f : 0f) - (player.controlUp ? 1f : 0f);

		return result;
	}

	public static Vector2 LookDirection(this Player player)
		=> (player.GetModPlayer<PlayerDirectioning>().MouseWorld - player.Center).SafeNormalize(Vector2.UnitY);

	// (De)buffs

	public static void RemoveBuffsOfType(this Player player, int type)
	{
		int buffIndex = player.FindBuffIndex(type);

		if (buffIndex >= 0) {
			player.DelBuff(buffIndex);
		}
	}

	// Inventory

	public static bool HasAccessory(this Player player, int itemId)
		=> player.EnumerateAccessories().Any(tuple => tuple.item.type == itemId);

	public static bool HasAccessory(this Player player, bool any, params int[] itemIds)
	{
		var accessories = player.EnumerateAccessories();

		bool Predicate((Item, int) tuple) => itemIds.Contains(tuple.Item1.type);

		return any ? accessories.Any(Predicate) : accessories.All(Predicate);
	}

	public static IEnumerable<(Item item, int index)> EnumerateAccessories(this Player player)
	{
		//TODO: Might need to update this in the future.
		for (int i = 3; i < 10; i++) {
			var item = player.armor[i];

			if (item != null && item.active) {
				yield return (item, i);
			}
		}
	}

	// Grappling hooks

	public static void StopGrappling(this Player player, Projectile? exceptFor = null)
	{
		foreach (var (grapplingHook, hookIndex) in player.EnumerateGrapplingHooks()) {
			if (grapplingHook != exceptFor && grapplingHook.ai[0] == 2f) {
				grapplingHook.Kill();

				player.grappling[hookIndex] = -1;
			}
		}
	}

	public static IEnumerable<(Projectile projectile, int hookIndex)> EnumerateGrapplingHooks(this Player player)
	{
		// The player.grappling array is some really useless crap.

		for (int i = 0; i < Main.projectile.Length; i++) {
			var proj = Main.projectile[i];

			if (proj != null && proj.active && proj.aiStyle == 7 && proj.owner == player.whoAmI) {
				yield return (proj, i);
			}
		}
	}
}
