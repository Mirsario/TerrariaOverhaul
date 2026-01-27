// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.AudioEffects;

internal sealed class UnderwaterMuffling : ModSystem
{
	public static readonly ConfigEntry<bool> EnableUnderwaterMuffling = new(ConfigSide.ClientOnly, true, "Ambience");

	private static float globalIntensity;

	public override void Load()
	{
		AudioEffectsSystem.OnSoundUpdate += OnSoundUpdate;
	}

	private static void OnSoundUpdate(Span<AudioEffectsSystem.SoundData> sounds)
	{
		if (!EnableUnderwaterMuffling) {
			globalIntensity = 0f;
			return;
		}

		const byte MinLocalLiquid = 128;
		const float MaxLocalIntensity = 1.0f;
		const float MaxGlobalIntensity = 1.0f;

		float goalGlobalIntensity = Main.LocalPlayer.CheckIfUnderwater() ? MaxGlobalIntensity : 0f;

		globalIntensity = MathUtils.StepTowards(globalIntensity, goalGlobalIntensity, 0.75f * TimeSystem.LogicDeltaTime);

		for (int i = 0; i < sounds.Length; i++) {
			ref var data = ref sounds[i];
			float localIntensity = 0f;

			// Only affects positional sounds.
			if ((data.TrackedSound?.TryGetTarget(out var sound)) == true && sound!.Position is Vector2 position) {
				// Sound position must be in liquid.
				if (Main.tile.TryGet(position.ToTileCoordinates(), out var startingTile) && startingTile.LiquidAmount >= MinLocalLiquid) {
					localIntensity += MaxLocalIntensity;
				}
			}

			data.Parameters.LowPassFiltering += localIntensity + globalIntensity;
		}
	}
}
