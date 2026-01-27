// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Common.ProjectileEffects;

[Autoload(Side = ModSide.Client)]
internal sealed class ProjectileScreenShake : GlobalProjectile
{
	private static readonly ContentSet Explosive = nameof(Explosive);

	public ScreenShake? ScreenShake { get; set; }

	public override bool InstancePerEntity => true;

	public override void SetDefaults(Projectile projectile)
	{
		if (Explosive.Has(projectile)) {
			ScreenShake = new ScreenShake(0.8f, 1.0f) {
				Range = 2048f,
			};
		}
	}

	public override void OnKill(Projectile projectile, int timeLeft)
	{
		if (ScreenShake is ScreenShake shake) {
			ScreenShakeSystem.New(shake, projectile.Center);
		}
	}
}
