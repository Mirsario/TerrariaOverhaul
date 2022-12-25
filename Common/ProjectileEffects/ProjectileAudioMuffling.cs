using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.AudioEffects;
using TerrariaOverhaul.Common.Tags;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.ProjectileEffects;

/// <summary>
/// Applies low pass filtering nearby whenever this projectile is destroyed.
/// </summary>
[Autoload(Side = ModSide.Client)]
public sealed class ProjectileAudioMuffling : GlobalProjectile
{
	public bool Enabled { get; set; }
	public float Range { get; set; } = 512f;
	public float MaxTimeInSeconds { get; set; } = 3f;

	public override bool InstancePerEntity => true;

	public override void SetDefaults(Projectile projectile)
	{
		if (OverhaulProjectileTags.Explosive.Has(projectile.type)) {
			Enabled = true;
			Range = 512f;
			MaxTimeInSeconds = 5f;
		}
	}

	public override void Kill(Projectile projectile, int timeLeft)
	{
		if (!Enabled) {
			return;
		}

		if (Main.LocalPlayer is not Player localPlayer) {
			return;
		}

		float distance = Vector2.Distance(localPlayer.Center, projectile.Center);
		int lowPassFilteringTime = (int)(TimeSystem.LogicFramerate * MathUtils.DistancePower(distance, Range));

		if (lowPassFilteringTime <= 0) {
			return;
		}

		AudioEffectsSystem.AddAudioEffectModifier(
			lowPassFilteringTime,
			$"{nameof(TerrariaOverhaul)}/{nameof(ProjectileAudioMuffling)}",
			(float intensity, ref AudioEffectParameters soundParameters, ref AudioEffectParameters musicParameters) => {
				float total = intensity * 0.5f;

				soundParameters.LowPassFiltering += total;
				musicParameters.LowPassFiltering += total;
			}
		);
	}
}
