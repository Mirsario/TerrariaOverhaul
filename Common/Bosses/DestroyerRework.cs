// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.Bosses;

public sealed class DestroyerRework : GlobalNPC
{
	private const string BaseModTexturePath = "Assets/Textures/Bosses";

	// Audio & Screenshake
	public static readonly ConfigEntry<bool> EnableDestroyerEffects = new(ConfigSide.ClientOnly, true, "Bosses");
	// Sprite Swap & Size Increase.
	public static readonly ConfigEntry<bool> EnableDestroyerMakeover = new(ConfigSide.Both, true, "Bosses");

	private static bool spriteSwapActive;

	public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
	{
		return npc.type is NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail;
	}

	public override void SetDefaults(NPC npc)
	{
		UpdateSpriteSwap(Mod, unloading: false);

		if (EnableDestroyerMakeover) {
			npc.width = 64;
			npc.height = 64;
		}

		if (!EnableDestroyerEffects) {
			return;
		}

		var wormPart = npc.type switch {
			NPCID.TheDestroyer => NpcWormEffects.PartType.Head,
			NPCID.TheDestroyerTail => NpcWormEffects.PartType.Tail,
			_ => NpcWormEffects.PartType.Body,
		};

		if (wormPart == NpcWormEffects.PartType.Head && npc.TryGetGlobalNPC(out NpcAudioEffects audioEffects)) {
			audioEffects.Data = new NpcAudioEffects.EffectData {
				// Approach
				ApproachSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/MonsterScream") {
					Volume = 0.50f,
					PitchVariance = 0.25f,
					Identifier = "TheDestroyer_Voice",
					SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				},
				ApproachScreenShake = new ScreenShake {
					Power = 0.75f,
					Range = 1384f,
					LengthInSeconds = 2.0f,
					UniqueId = "TheDestroyer_Approach",
				},
				// Movement
				MovementSoundVelocityPitching = (2.5f, 10f, -0.50f, 0.50f),
				MovementSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/DestroyerRumbleLoop") {
					Volume = 0.525f,
					IsLooped = true,
					Identifier = "TheDestroyer_Movement",
				},
				// Random
				RandomSoundCooldown = (45, 60 * 10),
				RandomSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/RoboticScream1") {
					Volume = 0.85f,
					PitchVariance = 0.25f,
					Identifier = "TheDestroyer_Voice",
					SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest,
				},
			};
		}

		if (npc.TryGetGlobalNPC(out NpcWormEffects wormEffects)) {
			wormEffects.Data = new NpcWormEffects.EffectData {
				Type = wormPart,
				// Surfacing
				SurfacingScreenShake = new ScreenShake {
					Power = 0.40f,
					Range = 1024f,
					LengthInSeconds = 0.5f,
					UniqueId = $"TheDestroyer_{npc.whoAmI}",
				},
				DebrisStyle = new() {
					RangeInPixels = 64f,
				},
			};
			npc.HitSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/MetalNPCHitSound") {
				Pitch = -0.5f,
				PitchVariance = 0.3f,
			};

			npc.DeathSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/MetalNPCDeathSound") {
				Pitch = 0.5f,
				PitchVariance = 0.3f,
			};
		}
	}

	public override void Unload()
	{
		UpdateSpriteSwap(Mod, unloading: true);
	}

	public override bool PreAI(NPC npc)
	{
		if (EnableDestroyerMakeover && spriteSwapActive) {
			// This scale is applied to modify the distance between the worm's joints.
			npc.scale = 1.75f;
		}

		return true;
	}

	public override void PostAI(NPC npc)
	{
		if (EnableDestroyerMakeover && spriteSwapActive) {
			npc.scale = 1.00f;
		}
	}

	private static bool? UpdateSpriteSwap(Mod mod, bool unloading)
	{
		if (Main.dedServ) {
			return null;
		}

		bool shouldBeActive = !unloading && EnableDestroyerMakeover;

		if (spriteSwapActive != shouldBeActive) {
			if (shouldBeActive) {
				TextureAssets.Npc[NPCID.TheDestroyer] = mod.Assets.Request<Texture2D>($"{BaseModTexturePath}/TheDestroyer");
				TextureAssets.Npc[NPCID.TheDestroyerBody] = mod.Assets.Request<Texture2D>($"{BaseModTexturePath}/TheDestroyerBody");
				TextureAssets.Npc[NPCID.TheDestroyerTail] = mod.Assets.Request<Texture2D>($"{BaseModTexturePath}/TheDestroyerTail");
				TextureAssets.Dest[0] = mod.Assets.Request<Texture2D>($"{BaseModTexturePath}/TheDestroyer_Glow");
				TextureAssets.Dest[1] = mod.Assets.Request<Texture2D>($"{BaseModTexturePath}/TheDestroyerBody_Glow");
				TextureAssets.Dest[2] = mod.Assets.Request<Texture2D>($"{BaseModTexturePath}/TheDestroyerTail_Glow");
				TextureAssets.Gore[156] = mod.Assets.Request<Texture2D>($"{BaseModTexturePath}/TheDestroyerGore");
			} else {
				TextureAssets.Npc[NPCID.TheDestroyer] = Main.Assets.Request<Texture2D>($"Images/NPC_{NPCID.TheDestroyer}");
				TextureAssets.Npc[NPCID.TheDestroyerBody] = Main.Assets.Request<Texture2D>($"Images/NPC_{NPCID.TheDestroyerBody}");
				TextureAssets.Npc[NPCID.TheDestroyerTail] = Main.Assets.Request<Texture2D>($"Images/NPC_{NPCID.TheDestroyerTail}");
				TextureAssets.Dest[0] = Main.Assets.Request<Texture2D>($"Images/Dest1");
				TextureAssets.Dest[1] = Main.Assets.Request<Texture2D>($"Images/Dest2");
				TextureAssets.Dest[2] = Main.Assets.Request<Texture2D>($"Images/Dest3");
				TextureAssets.Gore[156] = Main.Assets.Request<Texture2D>($"Images/Gore_156");
			}

			return spriteSwapActive = shouldBeActive;
		}

		return null;
	}
}

/*public sealed class ProbeSoundRework : GlobalNPC
{
	public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type is NPCID.Probe;

	public override void SetDefaults(NPC npc)
	{
		npc.HitSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/MetalNPCHitSound") {
			Pitch = 0.5f,
			PitchVariance = 0.3f,
		};
		npc.DeathSound = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/MetalNPCDeathSound") {
			Pitch = 0.5f,
			PitchVariance = 0.3f,
		};
	}

	public override void PostAI(NPC npc)
	{
		if (npc.active) {
			if (npc.ai[0] % 180 == 0) {
				SoundEngine.PlaySound(new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Bosses/MetalNPCIdleSound") {
					MaxInstances = 3,
					PitchVariance = 0.1f,
				}, npc.position);
			}
		}
	}
}*/
