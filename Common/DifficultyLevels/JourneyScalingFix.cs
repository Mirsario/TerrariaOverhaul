using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using ASharedSliderPower = Terraria.GameContent.Creative.CreativePowers.ASharedSliderPower;
using DifficultySliderPower = Terraria.GameContent.Creative.CreativePowers.DifficultySliderPower;

namespace TerrariaOverhaul.Common.DifficultyLevels;

/// <summary>
/// Makes the journey mode difficulty slider actually use values from the Main.RegisteredGameModes collection.
/// </summary>
internal sealed class JourneyScalingFix : ILoadable
{
	private const int VanillaGameModeCount = 4;
	private const int MaxVanillaGameModeIndex = VanillaGameModeCount - 1;

	private static FieldInfo? valueField;

	void ILoadable.Load(Mod mod)
	{
		valueField = typeof(ASharedSliderPower).GetField("_sliderCurrentValueCache", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!;

		On.Terraria.NPC.ScaleStats += OnNpcScaleStats;
		IL.Terraria.Projectile.Damage += ProjectileDamageInjection;
	}

	void ILoadable.Unload()
	{

	}

	private static void OnNpcScaleStats(On.Terraria.NPC.orig_ScaleStats orig, NPC npc, int? activePlayersCount, GameModeData gameModeData, float? strengthOverride)
	{
		// Pass in GameModeData modified depending on journey settings.
		ModifyGameModeData(ref gameModeData);

		// Prevent vanilla journey mode scaling code from running.
		gameModeData.IsJourneyMode = false;
		strengthOverride ??= 1f;

		orig(npc, activePlayersCount, gameModeData, strengthOverride);
	}

	private static void ProjectileDamageInjection(ILContext context)
	{
		var il = new ILCursor(context);

		// Match every call and store of 'Main.GameModeInfo'.
		int gameModeLocalId = -1;

		while (il.TryGotoNext(
			MoveType.After,
			i => i.MatchCall(typeof(Main), $"get_{nameof(Main.GameModeInfo)}"),
			i => i.MatchStloc(out gameModeLocalId)
		)) {
			il.Emit(OpCodes.Ldloca, gameModeLocalId);
			il.EmitDelegate(ModifyGameModeData);
		}
	}

	private static void ModifyGameModeData(ref GameModeData gameMode)
	{
		if (!gameMode.IsJourneyMode) {
			return;
		}

		var power = CreativePowerManager.Instance.GetPower<DifficultySliderPower>();

		if (power.GetIsUnlocked()) {
			float value = (float)valueField!.GetValue(power)!;
			float multipliedValue = MathUtils.Clamp01(value) * MaxVanillaGameModeIndex;
			int lowerDifficultyIndex = (int)MathF.Floor(multipliedValue);
			int higherDifficultyIndex = (int)MathF.Ceiling(multipliedValue);

			if (lowerDifficultyIndex == higherDifficultyIndex) {
				gameMode = Main.RegisteredGameModes[lowerDifficultyIndex];
			} else {
				var gameModeA = Main.RegisteredGameModes[DifficultyIndexToId(lowerDifficultyIndex)];
				var gameModeB = Main.RegisteredGameModes[DifficultyIndexToId(higherDifficultyIndex)];

				float stepValue = MathUtils.Clamp01(multipliedValue - MathF.Floor(multipliedValue));

				gameMode = MixGameModes(in gameModeA, gameModeB, stepValue);
			}
		}
	}

	private static int DifficultyIndexToId(int index)
	{
		return MathUtils.Modulo(index - 1, MaxVanillaGameModeIndex);
	}

	private static GameModeData MixGameModes(in GameModeData a, in GameModeData b, float step)
	{
		var result = a;

		result.EnemyMaxLifeMultiplier = MathHelper.Lerp(a.EnemyMaxLifeMultiplier, b.EnemyMaxLifeMultiplier, step);
		result.EnemyDamageMultiplier = MathHelper.Lerp(a.EnemyDamageMultiplier, b.EnemyDamageMultiplier, step);
		result.DebuffTimeMultiplier = MathHelper.Lerp(a.DebuffTimeMultiplier, b.DebuffTimeMultiplier, step);
		result.KnockbackToEnemiesMultiplier = MathHelper.Lerp(a.KnockbackToEnemiesMultiplier, b.KnockbackToEnemiesMultiplier, step);
		result.TownNPCDamageMultiplier = MathHelper.Lerp(a.TownNPCDamageMultiplier, b.TownNPCDamageMultiplier, step);
		result.EnemyDefenseMultiplier = MathHelper.Lerp(a.EnemyDefenseMultiplier, b.EnemyDefenseMultiplier, step);
		result.EnemyMoneyDropMultiplier = MathHelper.Lerp(a.EnemyMoneyDropMultiplier, b.EnemyMoneyDropMultiplier, step);

		return result;
	}
}
