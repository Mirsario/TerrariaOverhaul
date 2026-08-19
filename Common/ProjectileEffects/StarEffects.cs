// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.


using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Core.AudioEffects;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.ProjectileEffects;

[Autoload(Side = ModSide.Client)]
internal sealed class StarEffects : GlobalProjectile
{
	private bool spawnedSound;

	public override bool InstancePerEntity => true;
	
	public override bool AppliesToEntity(Projectile projectile, bool lateInstantiation)
	{
		return projectile.type
			is ProjectileID.FallingStar
			or ProjectileID.StarCannonStar
			or ProjectileID.SuperStar
			or ProjectileID.HallowStar
			or ProjectileID.StarVeilStar
			or ProjectileID.StarCloakStar;
	}

	public override void Load()
	{
		AudioEffectsSystem.OnSoundPlay += OnPlay;
	}

	private static void OnPlay(ref SoundStyle style)
	{
		// Reduce sound volume of vanilla star sounds.
		if (style == SoundID.Item9) {
			style.Volume *= 0.1f;
			style.Pitch = Main.rand.NextFloat(-0.1f, +0.2f);
		}
	}

	public override void AI(Projectile projectile)
	{
		if (!spawnedSound) {
			var tracker = new ProjectileTracker(projectile);
			var style = new SoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Projectiles/StarBurn") {
				Volume = 0.07f,
				PitchVariance = 0.2f,
				MaxInstances = 3,
				IsLooped = true,
				PauseBehavior = PauseBehavior.PauseWithGame,
			};
			SoundEngine.PlaySound(style, projectile.Center, tracker.AudioCallback);
			spawnedSound = true;
		}
	}

	public override void OnKill(Projectile projectile, int timeLeft)
	{
		if (Main.dedServ) return;

		SoundEngine.PlaySound(position: projectile.Center, style: new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Projectiles/MagicMissile", 3) {
			Volume = 0.25f,
			PitchVariance = 0.35f,
			MaxInstances = 3,
		});
		ScreenShakeSystem.New(new() {
			Power = projectile.type is ProjectileID.FallingStar ? 0.10f : 0.02f,
			Range = projectile.type is ProjectileID.FallingStar ? 4096 : 768,
			LengthInSeconds = 0.6f,
		}, projectile.Center);
	}
}
