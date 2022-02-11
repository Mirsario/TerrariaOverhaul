using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.ItemComponents;

namespace TerrariaOverhaul.Common.Recoil
{
	// Very experimental feature.

	[Autoload(Side = ModSide.Client)]
	public sealed class ItemAimRecoil : ItemComponent
	{
		public float RecoilMultiplier { get; set; } = 1f;

		public override bool? UseItem(Item item, Player player)
		{
			if (Enabled) {
				ApplyRecoil(item, player);
			}

			return base.UseItem(item, player);
		}

		public void ApplyRecoil(Item item, Player player)
		{
			float strength = GetRecoilStrength(item);

			if (strength <= 0f) {
				return;
			}

			var mousePos = new Vector2(Main.mouseX, Main.mouseY);
			var origin = player.Center - Main.screenPosition;

			strength *= 1f - MathHelper.Clamp(Vector2.Distance(mousePos, origin) / Main.screenWidth, 0f, 1f);

			var offset = mousePos.RotatedBy(MathHelper.ToRadians(player.direction * -strength), origin) - mousePos;

			RecoilSystem.AddCursorOffset(offset, 20f);
		}

		public float GetRecoilStrength(Item item)
		{
			float baseRecoil = Math.Min(item.useTime * 0.1f, 2.3f);

			return baseRecoil * RecoilMultiplier;
		}
	}
}
