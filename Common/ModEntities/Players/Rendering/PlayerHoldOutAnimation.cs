using System;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Common.ModEntities.Players.Rendering
{
	public sealed class PlayerHoldOutAnimation : PlayerBase
	{
		private float itemRotation;
		private float targetItemRotation;

		private bool ForceUseAnim(Item item) => item.useStyle == ItemUseStyleID.Shoot && !item.noUseGraphic;

		public override void Load()
		{
			On.Terraria.Player.ItemCheck_ApplyHoldStyle += (orig, player, mountOffset, sItem, heldItemFrame) => {
				if(ForceUseAnim(sItem)) {
					player.ItemCheck_ApplyUseStyle(mountOffset, sItem, heldItemFrame);

					return;
				}

				orig(player, mountOffset, sItem, heldItemFrame);
			};

			On.Terraria.Player.ItemCheck_ApplyUseStyle += (orig, player, mountOffset, sItem, heldItemFrame) => {
				orig(player, mountOffset, sItem, heldItemFrame);

				if(sItem.useStyle == ItemUseStyleID.Shoot) {
					player.itemRotation = player.GetModPlayer<PlayerHoldOutAnimation>().itemRotation;
				}
			};

			On.Terraria.Player.PlayerFrame += (orig, player) => {
				if(ForceUseAnim(player.HeldItem) && player.itemAnimation <= 0) {
					InvokeWithForcedAnimation(player, () => orig(player));
					return;
				}

				orig(player);
			};

			On.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_27_HeldItem += (On.Terraria.DataStructures.PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawInfo) => {
				var player = drawInfo.drawPlayer;

				if(ForceUseAnim(player.HeldItem) && player.itemAnimation <= 0) {
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
			var mouseWorld = player.GetModPlayer<PlayerDirectioning>().mouseWorld;
			Vector2 offset = mouseWorld - player.Center;

			if(Math.Sign(offset.X) == player.direction) {
				targetItemRotation = (offset * player.direction).ToRotation();
			}

			itemRotation = MathHelper.Lerp(itemRotation, targetItemRotation, 16f * Systems.Time.TimeSystem.LogicDeltaTime);

			//This could go somewhere else?
			if(player.HeldItem?.IsAir == false && ForceUseAnim(player.HeldItem)) {
				player.HeldItem.useTurn = true;
			}
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
}
