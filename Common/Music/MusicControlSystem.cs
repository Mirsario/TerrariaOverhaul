using Microsoft.Xna.Framework.Audio;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Music;

// This system:
// - Improves music fading.
// - Replaces music stopping with pausing in some cases.
// - Hooks other systems for modification of music volume and etc.
public sealed class MusicControlSystem : ModSystem
{
	public delegate void TrackUpdateCallback(bool isActiveTrack, int trackIndex, ref float musicVolume, ref float musicFade);

	private const string VolumeVariable = "Volume";

	public static readonly ConfigEntry<bool> EnableMusicPlaybackPositionPreservation = new(ConfigSide.ClientOnly, "Music", nameof(EnableMusicPlaybackPositionPreservation), () => true);

	public static event TrackUpdateCallback? OnTrackUpdate;

	public override void Load()
	{
		Main.QueueMainThreadAction(() => {
			IL.Terraria.Main.UpdateAudio += UpdateAudioInjection;
		});
	}

	public override void Unload()
	{
		OnTrackUpdate = null;
	}

	private static void UpdateAudioInjection(ILContext context)
	{
		var il = new ILCursor(context);

		int iLocalId = 0;
		int volumeLocalId = 0;
		int fadeLocalId = 0;
		ILLabel? skipLabel = null;

		// Match 'audioSystem.UpdateCommonTrack(isActive, i, num2, ref tempFade);'
		il.GotoNext(
			MoveType.Before,
			i => i.MatchLdsfld(typeof(Main), nameof(Main.audioSystem)),
			i => i.MatchLdloc(out _), // 'isActive' (the game, that is)
			i => i.MatchLdloc(out iLocalId), // i
			i => i.MatchLdloc(out volumeLocalId), // totalVolume
			i => i.MatchLdloca(out fadeLocalId), // tempFade
			i => i.MatchCallvirt(typeof(IAudioSystem), nameof(IAudioSystem.UpdateCommonTrack))
		);

		// Grab the skipping label. This is separate because of possibility of a debug no-op.
		il.GotoNext(
			MoveType.Before,
			i => i.MatchBr(out skipLabel)
		);

		// Match 'if (i == curMusic)' above the previous line

		il.GotoPrev(
			MoveType.Before,
			i => i.MatchLdloc(out iLocalId),
			i => i.MatchLdsfld(typeof(Main), nameof(Main.curMusic))//,
			//i => i.MatchBneUn(out _)
		);

		il.HijackIncomingLabels();

		// Insert our code
		il.Emit(OpCodes.Ldsfld, typeof(Main).GetField(nameof(Main.audioSystem))!);
		il.Emit(OpCodes.Ldloc, iLocalId);
		il.Emit(OpCodes.Ldloc, volumeLocalId);
		il.Emit(OpCodes.Ldloca, fadeLocalId);
		il.EmitDelegate(UpdateActiveTrack);

		// Jump over vanilla code
		il.Emit(OpCodes.Br, skipLabel!);
	}

	private static void UpdateActiveTrack(LegacyAudioSystem system, int trackIndex, float trackVolume, ref float trackFade)
	{
		if (!system.WaveBank.IsPrepared) {
			return;
		}

		var audioTrack = system.AudioTracks[trackIndex];

		if (audioTrack == null) {
			return;
		}

		bool isActiveTrack = trackIndex == Main.curMusic;

		// Fade
		float targetFade = isActiveTrack ? 1f : 0f;

		trackFade = MathUtils.StepTowards(trackFade, targetFade, 0.5f * TimeSystem.LogicDeltaTime);

		// Audio track update
		bool shouldBePlaying = trackVolume > 0f;

		OnTrackUpdate?.Invoke(isActiveTrack, trackIndex, ref trackVolume, ref trackFade);

		audioTrack.SetVariable(VolumeVariable, trackVolume);

		// Start playback
		if (shouldBePlaying && !audioTrack.IsPlaying) {
			if (audioTrack.IsStopped) {
				audioTrack.Reuse();
			}

			audioTrack.Play();
		}

		// Stop / pause playback
		if (!shouldBePlaying && trackVolume <= 0f && trackFade <= 0f && audioTrack.IsPlaying) {
			if (Main.musicVolume <= 0f || !EnableMusicPlaybackPositionPreservation) {
				audioTrack.Stop(AudioStopOptions.Immediate);
			} else {
				audioTrack.Pause();
			}
		}
	}
}
