﻿using System;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience;

public abstract class AmbienceTrack : ModType
{
	private bool soundDefined;
	private SoundStyle sound;
	private SlotId instanceReference;

	public SignalContainer[] Signals { get; protected set; } = Array.Empty<SignalContainer>();
	public bool IsLooped { get; protected set; }
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

	public float GetTargetVolume()
	{
		float volume = 1f;

		for (int i = 0; i < Signals.Length; i++) {
			ref readonly var container = ref Signals[i];
			var flags = container.Flags;
			var signals = container.Signals;

			float multiplier = 0f;

			for (int j = 0; j < signals.Length; j++) {
				float value = EnvironmentSystem.GetSignal(signals[j]);

				if ((flags & SignalFlags.Inverse) != 0) {
					value = 1f - value;
				}

				if (j == 0) {
					multiplier = value;
				}

				// dumb
				if ((flags & SignalFlags.Max) != 0) {
					multiplier = Math.Max(multiplier, value);
				} else if ((flags & SignalFlags.Min) != 0) {
					multiplier = Math.Min(multiplier, value);
				} else {
					multiplier *= value;
				}
			}

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
		float targetVolume = GetTargetVolume();

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
}
