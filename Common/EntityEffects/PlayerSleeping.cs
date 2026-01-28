// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Api.Camera;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.EntityEffects;

internal sealed class PlayerSleeping : ModPlayer
{
	public static readonly ConfigEntry<bool> EnableSleepingEffects = new(ConfigSide.ClientOnly, true, "Aesthetics");

	private bool wasSleeping;
	private SlotId loopInstance;
	private float loopVolume;
	
	public override void PostUpdate()
	{
		if (Main.dedServ) return;

		bool isSleeping = Player.sleeping.isSleeping && EnableSleepingEffects;

		if (isSleeping && Player.IsLocal()) {
			CameraCurios.Create(new() {
				Identifier = "Sleeping",
				Weight = 2f,
				LengthInSeconds = 0.2f,
				Position = Player.Center,
				Zoom = 2f,
			});
		}
		if (isSleeping && !wasSleeping) {
			SoundEngine.PlaySound(new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/GetInBed") {
				Volume = Player.IsLocal() ? 0.5f : 0.2f,
				PitchVariance = 0.2f,
			}, Player.Center);
		}

		float volumeTarget = (isSleeping ? 1f : 0f) * (Player.IsLocal() ? 1f : 0.5f);
		float halfStep = (isSleeping ? 0.01f : 0.5f) * TimeSystem.LogicDeltaTime;
		loopVolume = MathUtils.StepTowards(MathHelper.Lerp(loopVolume, volumeTarget, halfStep), volumeTarget, halfStep);
		SoundUtils.UpdateLoopingSound(ref loopInstance, Player.Center, loopVolume, new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Player/Sleeping") {
			Volume = 0.1f,
			IsLooped = true,
		});

		wasSleeping = isSleeping;
	}
}
