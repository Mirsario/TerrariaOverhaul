using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerMiningHelmetLighting : PlayerBase
	{
		public override void Load()
		{
			On.Terraria.Player.UpdateArmorLights += (orig, player) => {
				if(player.head == ArmorIDs.Head.MiningHelmet) {
					player.head = -1;

					orig(player);

					player.head = ArmorIDs.Head.MiningHelmet;

					return;
				}

				orig(player);
			};
		}

		public override void PostUpdate()
		{
			if(Player.armor[0] == null || Player.armor[0].headSlot != ArmorIDs.Head.MiningHelmet) {
				return;
			}

			const int NumSteps = 24;

			var mouseWorld = Player.GetModPlayer<PlayerDirectioning>().mouseWorld;
			var startPos = Player.Center - Vector2.UnitY * 8;
			var endPos = Player.position + Vector2.Transform(new Vector2(NumSteps * 16f, 0f), Matrix.CreateRotationZ((mouseWorld - startPos).ToRotation()));
			float maxBrightness = 1f;
			var lightColor = new Vector3(1f, 0.86f, 0.70f);

			Lighting.AddLight(startPos, lightColor * 0.25f);

			for(int i = 0; i < NumSteps; i++) {
				var currentPos = Vector2.Lerp(startPos, endPos, i / (float)NumSteps);

				if(!Main.tile.TryGet(currentPos.ToTileCoordinates16(), out var tile)) {
					continue;
				}

				if(tile.active() && Main.tileSolid[tile.type]) {
					maxBrightness -= 0.2f;

					if(maxBrightness <= 0f) {
						break;
					}
				}

				float brightness = MathHelper.Clamp(i / (float)NumSteps, 0.1f, 1f) * maxBrightness;

				Lighting.AddLight(currentPos, lightColor * brightness);
			}
		}
	}
}
