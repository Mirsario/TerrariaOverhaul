using System;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerItemRotation : OverhaulPlayer
	{
		public float? forcedItemRotation;

		public override void PostUpdate()
		{
			if(forcedItemRotation.HasValue) {
				player.itemRotation = forcedItemRotation.Value;

				forcedItemRotation = null;
			}
		}
	}
}
