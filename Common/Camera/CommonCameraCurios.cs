// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using ReLogic.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

public sealed class CommonCameraCurios : ModSystem
{
	private static readonly ConfigEntry<bool> FocusCameraOnBosses = new(ConfigSide.ClientOnly, true, "Camera");
	private static readonly ConfigEntry<bool> FocusCameraOnRareEnemies = new(ConfigSide.ClientOnly, true, "Camera");
	private static readonly ConfigEntry<bool> FocusCameraOnRareFriendlies = new(ConfigSide.ClientOnly, true, "Camera");
	private static readonly ConfigEntry<bool> FocusCameraOnRarePickups = new(ConfigSide.ClientOnly, true, "Camera");

	public override void PostUpdateNPCs()
	{
		var player = Main.LocalPlayer;
		var playerCenter = player.Center;
		bool lifeAnalyzer = player.accCritterGuide;
		var bossPosition = new WeightedValue<Vector2D>(default, 0.0);
		var rarePosition = new WeightedValue<Vector2D>(default, 0.0);

		var baseCurio = new CameraCurio() {
			Weight = float.NaN,
			LengthInSeconds = 0.01f,
			FadeInLength = 2.0f,
			FadeOutLength = 1.0f,
		};
		var bossCurio = baseCurio with {
			UniqueId =  player.dead ? "BossesWhileDead" : "Bosses",
			Weight = player.dead ? 1.00f : 0.15f,
			Range = new(Min: 512f, Max: 1536f, Exponent: 2f),
		};
		var rareCurio = baseCurio with {
			UniqueId = "RareNPCs",
			Weight = 0.15f,
			Range = lifeAnalyzer ? new(Min: 512f, Max: 2048f, Exponent: 2.0f) : new(Min: 64f, Max: 768f, Exponent: 2.0f),
			Zoom = +0.125f,
		};

		foreach (var npc in Main.ActiveNPCs) {
			bool hostile = !npc.friendly && npc.damage > 0;
			bool mainBoss = npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
			bool secondaryBoss = npc.realLife >= 0 && Main.npc[npc.realLife] is { active: true, boss: true };

			if ((mainBoss || secondaryBoss) && FocusCameraOnBosses) {
				float mul = mainBoss ? 1f : 0.1f;
				if (bossCurio.Range.Value.DistanceFactor(npc.Distance(playerCenter)) is > 0f and float weight)
					bossPosition.Add(npc.Center.ToF64(), weight * mul);
				continue;
			}
			
			if (npc.rarity > 0 && (hostile ? FocusCameraOnRareEnemies : FocusCameraOnRareFriendlies)) {
				if (rareCurio.Range.Value.DistanceFactor(npc.Distance(playerCenter)) is > 0f and float weight)
					rarePosition.Add(npc.Center.ToF64(), weight);
				continue;
			}
		}

		if (bossPosition.TotalWeight > 0f) CameraCurios.Create(bossPosition.Total().ToF32(), bossCurio);
		if (rarePosition.TotalWeight > 0f) CameraCurios.Create(rarePosition.Total().ToF32(), rareCurio);
	}

	public override void PostUpdateItems()
	{
		if (!FocusCameraOnRarePickups) {
			return;
		}

		CameraCurio curio;
		curio.Zoom = +0.125f;
		curio.Range = new(Min: 100f, Max: 450f, Exponent: 1.50f);
		curio.Weight = 0.325f;
		curio.LengthInSeconds = 0.25f;
		curio.FadeInLength = 1.0f;
		curio.FadeOutLength = 1.0f;

		var playerCenter = Main.LocalPlayer.Center;
		var position = new WeightedValue<Vector2D>(default, 0.0);

		foreach (var item in Main.ActiveItems) {
			if (!ItemID.Sets.BossBag[item.type] && item.rare is not ItemRarityID.Quest or ItemRarityID.Expert or ItemRarityID.Master) {
				continue;
			}

			if (curio.Range.Value.DistanceFactor(item.Distance(playerCenter)) is > 0f and float weight) {
				position.Add(item.Center.ToF64(), weight);
			}
		}

		if (position.TotalWeight > 0f) {
			curio.UniqueId = "Loot";
			CameraCurios.Create(position.Total().ToF32(), curio);
		}
	}
}
