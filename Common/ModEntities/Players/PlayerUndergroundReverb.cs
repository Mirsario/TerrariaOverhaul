using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.AudioEffects;
using TerrariaOverhaul.Utilities.DataStructures;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	[Autoload(Side = ModSide.Client)]
	public sealed class PlayerUndergroundReverb : ModPlayer
	{
		public const float MaxReverbIntensity = 0.7f;
		public static readonly Gradient<float> DepthToReverbIntensityGradient = new(
			(0f, 0f),
			((float)Main.worldSurface, 0f),
			((float)Main.worldSurface * 1.1f, 1f),
			((float)Main.rockLayer, 1f)
		);

		public override void PostUpdate()
		{
			if(!Player.IsLocal()) {
				return;
			}

			float intensity = DepthToReverbIntensityGradient.GetValue(Player.position.ToTileCoordinates().Y) * MaxReverbIntensity;

			if(intensity > 0f) {
				AudioEffectsSystem.AddAudioEffectModifier(
					60,
					$"{nameof(TerrariaOverhaul)}/{nameof(PlayerUndergroundReverb)}",
					(float intensity, ref float reverbIntensity, ref float _) => reverbIntensity += intensity * intensity
				);
			}
		}
	}
}
