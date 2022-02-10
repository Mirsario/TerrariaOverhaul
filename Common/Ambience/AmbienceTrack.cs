using System;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Ambience
{
	public abstract class AmbienceTrack : ModType
	{
		public bool IsLooped { get; protected set; }
		public ISoundStyle Sound { get; protected set; }
		public float Volume { get; private set; }
		public SlotId InstanceReference { get; private set; }

		public virtual float VolumeChangeSpeed => 0.5f;

		private bool ShouldBeActive => Volume > 0f && AmbienceSystem.EnableAmbientSounds;

		public abstract void Initialize();
		public abstract float GetTargetVolume(Player localPlayer);

		protected override void Register()
		{
			Initialize();

			if (Sound == null) {
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
			var soundInstance = InstanceReference.IsValid ? SoundEngine.GetActiveSound(InstanceReference) : null;

			if (!ShouldBeActive) {
				if (soundInstance != null) {
					soundInstance.Stop();

					InstanceReference = SlotId.Invalid;
				}

				return;
			}

			if (soundInstance == null) {
				var sound = Sound;
				float styleVolume = sound.Volume;

				// The need to do this is horrible.
				if (sound is ModSoundStyle modSound1) {
					modSound1.Volume = Volume;
				}

				InstanceReference = SoundEngine.PlayTrackedSound(sound, CameraSystem.ScreenCenter);

				if (sound is ModSoundStyle modSound2) {
					modSound2.Volume = styleVolume;
				}
			} else {
				soundInstance.Volume = Volume;
				soundInstance.Position = CameraSystem.ScreenCenter;
			}
		}
	}
}
