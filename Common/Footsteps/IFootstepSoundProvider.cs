using Terraria.Audio;

namespace TerrariaOverhaul.Common.Footsteps;

public interface IFootstepSoundProvider
{
	SoundStyle? FootstepSound { get; }

	SoundStyle? JumpFootstepSound => FootstepSound;
	SoundStyle? LandFootstepSound => FootstepSound;
}
