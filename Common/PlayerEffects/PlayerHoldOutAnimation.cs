using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.PlayerEffects;

public sealed class PlayerHoldOutAnimation : ModPlayer
{
	public static readonly ConfigEntry<bool> AlwaysShowAimableWeapons = new(ConfigSide.ClientOnly, "PlayerVisuals", nameof(AlwaysShowAimableWeapons), () => true);

	private float directItemRotation;
	private float directTargetItemRotation;

	public float VisualRecoil { get; set; }

	public override void Load()
	{
		On_Player.ItemCheck_ApplyHoldStyle_Inner += static (orig, player, mountOffset, sItem, heldItemFrame) => {
			if (ShouldForceUseAnim(player, sItem)) {
				player.ItemCheck_ApplyUseStyle(mountOffset, sItem, heldItemFrame);

				return;
			}

			orig(player, mountOffset, sItem, heldItemFrame);
		};

		On_Player.ItemCheck_ApplyUseStyle_Inner += static (orig, player, mountOffset, sItem, heldItemFrame) => {
			orig(player, mountOffset, sItem, heldItemFrame);

			if (sItem.useStyle == ItemUseStyleID.Shoot) {
				var modPlayer = player.GetModPlayer<PlayerHoldOutAnimation>();

				player.itemRotation = ConvertRotation(modPlayer.directItemRotation, player) - MathHelper.ToRadians(modPlayer.VisualRecoil * player.direction * (int)player.gravDir);

				// Fix rotation range.
				if (player.itemRotation > MathHelper.Pi) {
					player.itemRotation -= MathHelper.TwoPi;
				}

				if (player.itemRotation < -MathHelper.Pi) {
					player.itemRotation += MathHelper.TwoPi;
				}
			}
		};

		On_Player.PlayerFrame += static (orig, player) => {
			if (ShouldForceUseAnim(player, player.HeldItem) && player.itemAnimation <= 0 && AlwaysShowAimableWeapons) {
				InvokeWithForcedAnimation(player, () => orig(player));
				return;
			}

			orig(player);
		};

		On_PlayerDrawLayers.DrawPlayer_27_HeldItem += static (On_PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawInfo) => {
			var player = drawInfo.drawPlayer;

			if (ShouldForceUseAnim(player, player.HeldItem) && player.itemAnimation <= 0 && AlwaysShowAimableWeapons) {
				ForceAnim(player, out int itemAnim, out int itemAnimMax);

				orig(ref drawInfo);

				RestoreAnim(player, itemAnim, itemAnimMax);

				return;
			}

			orig(ref drawInfo);
		};
	}

	public override void PreUpdate()
	{
		var mouseWorld = Player.GetModPlayer<PlayerDirectioning>().LookPosition;
		Vector2 offset = mouseWorld - Player.Center;
		int direction = offset.X >= 0f ? 1 : -1;

		if (offset != Vector2.Zero && direction == Player.direction) {
			directTargetItemRotation = offset.ToRotation();
		}

		directItemRotation = MathUtils.LerpRadians(directItemRotation, directTargetItemRotation, 16f * TimeSystem.LogicDeltaTime);
		VisualRecoil = MathHelper.Lerp(VisualRecoil, 0f, 10f * TimeSystem.LogicDeltaTime);

		// This could go somewhere else?
		if (Player.HeldItem?.IsAir == false && ShouldForceUseAnim(Player, Player.HeldItem)) {
			//TODO: Is this not ever reset? Looks like an undiscovered bug.
			Player.HeldItem.useTurn = true;
		}
	}

	private static bool ShouldForceUseAnim(Player player, Item item)
	{
		if (item.noUseGraphic) {
			return false;
		}

		if (item.useStyle != ItemUseStyleID.Shoot) {
			return false;
		}

		if (player.TalkNPC is NPC { active: true } && !player.ItemAnimationActive) {
			return false;
		}

		if (ContentSampleUtils.TryGetItem(item.type, out var itemSample) && itemSample.useStyle != item.useStyle) {
			return false;
		}

		return true;
	}

	private static float ConvertRotation(float rotation, Player player)
	{
		if (player.direction < 0) {
			rotation -= MathHelper.Pi;
		}

		return rotation;
	}

	private static void ForceAnim(Player player, out int itemAnim, out int itemAnimMax)
	{
		itemAnim = player.itemAnimation;
		itemAnimMax = player.itemAnimationMax;

		player.itemAnimation = 1;
		player.itemAnimationMax = 2;
	}

	private static void RestoreAnim(Player player, int itemAnim, int itemAnimMax)
	{
		player.itemAnimation = itemAnim;
		player.itemAnimationMax = itemAnimMax;
	}

	private static void InvokeWithForcedAnimation(Player player, Action action)
	{
		ForceAnim(player, out int itemAnim, out int itemAnimMax);

		action();

		RestoreAnim(player, itemAnim, itemAnimMax);
	}
}
