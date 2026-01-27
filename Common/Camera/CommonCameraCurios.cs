// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Api.Camera;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.Camera;

internal sealed class CommonCameraCurios : ModSystem
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
			Identifier = null!,
			Position = Vector2.Zero,
			Weight = float.NaN,
			LengthInSeconds = 0.01f,
			FadeInLength = 2.0f,
			FadeOutLength = 1.0f,
		};
		var bossCurio = baseCurio with {
			Identifier =  player.dead ? "BossesWhileDead" : "Bosses",
			Weight = player.dead ? 1.00f : 0.15f,
			Range = new(Min: 512f, Max: 1536f, Exponent: 2f),
		};
		var rareCurio = baseCurio with {
			Identifier = "RareNPCs",
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

		if (bossPosition.TotalWeight > 0f) CameraCurios.Create(bossCurio with { Position = bossPosition.Total().ToF32() });
		if (rarePosition.TotalWeight > 0f) CameraCurios.Create(rareCurio with { Position = rarePosition.Total().ToF32() });
	}

	public override void PostUpdateItems()
	{
		if (!FocusCameraOnRarePickups) {
			return;
		}

		var curio = new CameraCurio {
			Identifier = "Loot",
			Position = Vector2.Zero,
			Zoom = +0.125f,
			Range = new(Min: 100f, Max: 450f, Exponent: 1.50f),
			Weight = 0.325f,
			LengthInSeconds = 0.25f,
			FadeInLength = 1.0f,
			FadeOutLength = 1.0f,
		};

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
			CameraCurios.Create(curio with { Position = position.Total().ToF32() });
		}
	}
}
