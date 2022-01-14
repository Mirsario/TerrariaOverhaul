using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.Systems.CursorOffsets;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Items.Overhauls.Guns
{
	public abstract class Gun : ItemOverhaul, IShowItemCrosshair
	{
		public int MuzzleflashTime { get; protected set; }

		public virtual float GetRecoilStrength(Item item, Player player)
		{
			float baseRecoil = Math.Min(item.useTime * 0.1f, 2.3f);
			float randomizedRecoil = baseRecoil;// * Main.rand.NextFloat(0.75f, 1.333f);

			return randomizedRecoil;
		}

		public override void HoldItem(Item item, Player player)
		{
			base.HoldItem(item, player);

			if (MuzzleflashTime > 0) {
				MuzzleflashTime--;
			}
		}

		public override bool? UseItem(Item item, Player player)
		{
			MuzzleflashTime = Math.Max(MuzzleflashTime, 5);

			if (!Main.dedServ && player.IsLocal()) {
				ApplyRecoil(item, player);
			}

			return base.UseItem(item, player);
		}

		public void SpawnCasings<T>(Player player, int amount = 1) where T : ModGore
		{
			var position = player.Center + new Vector2(player.direction > 0 ? 0f : -6f, -12f);
			IEntitySource entitySource = new EntitySource_ItemUse(player, item);

			for (int i = 0; i < amount; i++) {
				var velocity = player.velocity * 0.5f + new Vector2(Main.rand.NextFloat(1f) * -player.direction, Main.rand.NextFloat(-0.5f, -1.5f));

				Gore.NewGore(entitySource, position, velocity, ModContent.GoreType<T>());
			}
		}

		public bool ShowItemCrosshair(Item item, Player player) => true;

		protected void ApplyRecoil(Item item, Player player)
		{
			float strength = GetRecoilStrength(item, player);

			if (strength <= 0f) {
				return;
			}

			var mousePos = new Vector2(Main.mouseX, Main.mouseY);
			var origin = player.Center - Main.screenPosition;

			strength *= 1f - MathHelper.Clamp(Vector2.Distance(mousePos, origin) / Main.screenWidth, 0f, 1f);

			var offset = mousePos.RotatedBy(MathHelper.ToRadians(player.direction * -strength), origin) - mousePos;

			CursorOffsetSystem.AddCursorOffset(offset, 20f);
		}
	}
}
