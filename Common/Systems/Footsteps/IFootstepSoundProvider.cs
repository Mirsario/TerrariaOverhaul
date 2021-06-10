using Terraria.Audio;

namespace TerrariaOverhaul.Common.Systems.Footsteps
{
	public interface IFootstepSoundProvider
	{
		SoundStyle FootstepSound { get; }
		SoundStyle JumpFootstepSound => FootstepSound;
		SoundStyle LandFootstepSound => FootstepSound;
	}
}
