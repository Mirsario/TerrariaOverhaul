using Terraria.Audio;

namespace TerrariaOverhaul.Common.Systems.Footsteps
{
	public interface IFootstepSoundProvider
	{
		ISoundStyle FootstepSound { get; }
		ISoundStyle JumpFootstepSound => FootstepSound;
		ISoundStyle LandFootstepSound => FootstepSound;
	}
}
