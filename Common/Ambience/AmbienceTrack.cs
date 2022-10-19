using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience;

public abstract class AmbienceTrack : ModType
{
	private bool soundDefined;
	private SoundStyle sound;
	private SlotId instanceReference;

	public bool IsLooped { get; protected set; }
	public TagCondition[] Conditions { get; protected set; } = Array.Empty<TagCondition>();
	public VolumeMultiplier.Function[] VolumeMultipliers { get; protected set; } = Array.Empty<VolumeMultiplier.Function>();
	public float VolumeChangeSpeed { get; protected set; } = 0.5f;
	public float Volume { get; private set; }

	private bool ShouldBeActive => Volume > 0f && AmbienceSystem.EnableAmbientSounds;

	public SoundStyle Sound {
		get => sound;
		protected set {
			sound = value;
			soundDefined = true;
		}
	}

	public SlotId InstanceReference {
		get => instanceReference;
		private set => instanceReference = value;
	}

	public abstract void Initialize();

	public float GetTargetVolume(Player localPlayer)
	{
		// Conditions

		if (!CheckConditions()) {
			return 0f;
		}

		// Multipliers

		var context = new VolumeMultiplier.Context {
			Player = localPlayer,
			PlayerTilePosition = localPlayer.Center * TileUtils.PixelSizeInUnits,
		};

		float volume = 1f;

		for (int i = 0; i < VolumeMultipliers.Length; i++) {
			float multiplier = VolumeMultipliers[i](in context);

			if (multiplier == 0f) {
				return 0f;
			}

			volume = MathHelper.Clamp(volume * multiplier, 0f, 1f);
		}

		return volume;
	}

	protected override void Register()
	{
		Initialize();

		if (!soundDefined) {
			throw new InvalidOperationException($"'{nameof(AmbienceTrack)}.{nameof(Sound)}' has not been assigned in '{GetType().Name}.{nameof(Initialize)}()'.");
		}

		AmbienceSystem.RegisterAmbienceTrack(this);
	}

	internal void Update()
	{
		float targetVolume = GetTargetVolume(Main.LocalPlayer);

		Volume = MathUtils.StepTowards(Volume, targetVolume, VolumeChangeSpeed * TimeSystem.LogicDeltaTime);

		UpdateSound();
	}

	private void UpdateSound()
	{
		SoundEngine.TryGetActiveSound(InstanceReference, out var soundInstance);

		if (!ShouldBeActive) {
			if (soundInstance != null) {
				soundInstance.Stop();

				InstanceReference = SlotId.Invalid;
			}

			return;
		}

		SoundUtils.UpdateLoopingSound(ref instanceReference, Sound, Volume, CameraSystem.ScreenCenter);
	}

	private bool CheckConditions()
	{
		for (int i = 0; i < Conditions.Length; i++) {
			if (!Conditions[i].Check()) {
				return false;
			}
		}

		return true;
	}
}
