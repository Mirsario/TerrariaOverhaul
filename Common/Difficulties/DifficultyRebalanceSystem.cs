using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Difficulties;

internal sealed class DifficultyRebalanceSystem : ModSystem
{
	private bool isEnabled;
	private bool shouldBeEnabled = true; // TODO: Don't be lazy and replace with config entries

	public override void PreUpdateEntities()
	{
		if (isEnabled == shouldBeEnabled) {
			return;
		}

		// This will unfortunately reset any changes from other mods

		Span<GameModeData> presets = new GameModeData[4] {
			GameModeData.NormalMode,
			GameModeData.ExpertMode,
			GameModeData.MasterMode,
			GameModeData.CreativeMode
		};

		if (shouldBeEnabled) {
			ModifyDifficultyLevels(presets);
		}

		for (int i = 0; i < presets.Length; i++) {
			Main.RegisteredGameModes[i] = presets[i];
		}

		Main.GameMode = Main.GameMode;
		isEnabled = shouldBeEnabled;
	}

	private static void ModifyDifficultyLevels(Span<GameModeData> presets)
	{
		// Unify various important values to expert mode's or nearby.

		for (int i = 0; i < presets.Length; i++) {
			presets[i] = presets[i] with {
				// Enemy health & defense -- This straight up should never differ, both too low and too high ruin satisfaction.
				EnemyMaxLifeMultiplier = 2f,
				EnemyDefenseMultiplier = 1f,
				// Knockback - Lowering it too much could ruin combat stunts.
				KnockbackToEnemiesMultiplier = 1.0f,
				// Debuff Time - Why not just make them more deadly rather than double times?
				DebuffTimeMultiplier = 1.0f,
				// Money drops - Expert gives too much money, Normal has too much grind.
				EnemyMoneyDropMultiplier = 1.75f // From [ 1.0, 2.5, 2.5 ]
			};
		}

		ref var journey = ref presets[GameModeID.Creative];
		ref var normal = ref presets[GameModeID.Normal];
		ref var expert = ref presets[GameModeID.Expert];
		ref var master = ref presets[GameModeID.Master];

		journey.EnemyDamageMultiplier = normal.EnemyDamageMultiplier = 1.25f; // From 1.0
		expert.EnemyDamageMultiplier = 2.0f; // From 2.0
		master.EnemyDamageMultiplier = 3.25f; // From 3.0
	}
}
