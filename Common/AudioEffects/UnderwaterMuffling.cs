using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.AudioEffects;

public sealed class UnderwaterMuffling : ModSystem
{
	private static float globalIntensity;

	public override void Load()
	{
		AudioEffectsSystem.OnSoundUpdate += OnSoundUpdate;
	}

	private static void OnSoundUpdate(Span<AudioEffectsSystem.SoundData> sounds)
	{
		const byte MinLocalLiquid = 128;
		const float MaxLocalIntensity = 1.0f;
		const float MaxGlobalIntensity = 1.0f;

		float goalGlobalIntensity = Main.LocalPlayer.IsUnderwater() ? MaxGlobalIntensity : 0f;

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
