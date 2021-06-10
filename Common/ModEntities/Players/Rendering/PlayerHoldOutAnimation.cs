using System;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;
using TerrariaOverhaul.Utilities.Enums;

namespace TerrariaOverhaul.Common.ModEntities.Players.Rendering
{
	public sealed class PlayerHoldOutAnimation : PlayerBase
	{
		public float visualRecoil;

		private float directItemRotation;
		private float directTargetItemRotation;

		public override void Load()
		{
			On.Terraria.Player.ItemCheck_ApplyHoldStyle += (orig, player, mountOffset, sItem, heldItemFrame) => {
				if(ShouldForceUseAnim(player, sItem)) {
					player.ItemCheck_ApplyUseStyle(mountOffset, sItem, heldItemFrame);

					return;
				}

				orig(player, mountOffset, sItem, heldItemFrame);
			};

			On.Terraria.Player.ItemCheck_ApplyUseStyle += (orig, player, mountOffset, sItem, heldItemFrame) => {
				orig(player, mountOffset, sItem, heldItemFrame);

				if(sItem.useStyle == ItemUseStyleID.Shoot) {
					var modPlayer = player.GetModPlayer<PlayerHoldOutAnimation>();

					player.itemRotation = ConvertRotation(modPlayer.directItemRotation, player) - MathHelper.ToRadians(modPlayer.visualRecoil * player.direction * (int)player.gravDir);
				}
			};

			On.Terraria.Player.PlayerFrame += (orig, player) => {
				if(ShouldForceUseAnim(player, player.HeldItem) && player.itemAnimation <= 0) {
					InvokeWithForcedAnimation(player, () => orig(player));
					return;
				}

				orig(player);
			};

			On.Terraria.DataStructures.PlayerDrawLayers.DrawPlayer_27_HeldItem += (On.Terraria.DataStructures.PlayerDrawLayers.orig_DrawPlayer_27_HeldItem orig, ref PlayerDrawSet drawInfo) => {
				var player = drawInfo.drawPlayer;

				if(ShouldForceUseAnim(player, player.HeldItem) && player.itemAnimation <= 0) {
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
			var mouseWorld = Player.GetModPlayer<PlayerDirectioning>().mouseWorld;
			Vector2 offset = mouseWorld - Player.Center;

			if(offset != Vector2.Zero && Math.Sign(offset.X) == Player.direction) {
				directTargetItemRotation = offset.ToRotation();
			}

			directItemRotation = MathUtils.LerpRadians(directItemRotation, directTargetItemRotation, 16f * TimeSystem.LogicDeltaTime);
			visualRecoil = MathHelper.Lerp(visualRecoil, 0f, 10f * TimeSystem.LogicDeltaTime);

			//This could go somewhere else?
			if(Player.HeldItem?.IsAir == false && ShouldForceUseAnim(Player, Player.HeldItem)) {
				Player.HeldItem.useTurn = true;
			}
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			var player = drawInfo.drawPlayer;

			if(ShouldForceUseAnim(player, player.HeldItem)) {
				float itemRotation = ConvertRotation(directItemRotation, player) - MathHelper.ToRadians(visualRecoil * 0.65f * player.direction * (int)player.gravDir);

				drawInfo.usesCompositeTorso = true;
				drawInfo.usesCompositeFrontHandAcc = true;
				drawInfo.usesCompositeBackHandAcc = true;
				drawInfo.compFrontArmFrame = new Rectangle(280, 0, 40, 56);
				drawInfo.compBackArmFrame = new Rectangle(320, 0, 40, 56);
				drawInfo.compositeFrontArmRotation = itemRotation - MathHelper.ToRadians(65f) * Player.direction;
				drawInfo.compositeBackArmRotation = itemRotation - MathHelper.ToRadians(90f) * Player.direction;
			}
		}

		private static bool ShouldForceUseAnim(Player player, Item item)
		{
			if(item.type == ItemID.VortexBeater && player.InItemAnimation) {
				return true;
			}

			return item.useStyle == ItemUseStyleID.Shoot && !item.noUseGraphic;
		}

		private static float ConvertRotation(float rotation, Player player)
		{
			if(player.direction < 0) {
				return rotation - MathHelper.Pi;
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
}
