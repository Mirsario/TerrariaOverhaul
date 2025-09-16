// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaOverhaul.Common.Encounters;

internal sealed class EncounterMapLayer : ModMapLayer
{
	public override Position GetDefaultPosition() => new Before(IMapLayer.Pings);

	public override void Draw(ref MapOverlayDrawContext context, ref string text)
	{
		const float MinSqrDistanceToShow = 2048f * 2048f;

		var localPlayer = Main.LocalPlayer;

		foreach (ref readonly var instance in EnemyEncounters.Encounters) {
			ref readonly var encounter = ref instance.Encounter;

			if (instance.State == EncounterState.Completed) continue;

			if (localPlayer.DistanceSQ(encounter.ActivationOrigin) > MinSqrDistanceToShow) {
#if DEBUG
				if (!Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.NumPad3))
					continue;
#else
				continue;
#endif
			}

			const float ScaleIfNotSelected = 1f;
			const float ScaleIfSelected = ScaleIfNotSelected * 2f;

			var texture = TextureAssets.NpcHeadBoss[19].Value;
			var color = instance.State == EncounterState.InProgress ? Color.MediumVioletRed : Color.White;

			if (context.Draw(texture, encounter.ActivationOrigin.ToTileCoordinates().ToVector2(), color, new SpriteFrame(1, 1, 0, 0), ScaleIfNotSelected, ScaleIfSelected, Alignment.Center).IsMouseOver) {
				text = encounter.Waves.Length switch {
					2 => "Heavy Encounter",
					>= 3  => "Super Heavy Encounter",
					_ => "Encounter",
				};
			}
		}
	}
}
