using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.CursorOffsets;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Guns
{
	//TODO: Delete, replace with composition.
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
