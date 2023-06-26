using Microsoft.Xna.Framework.Graphics;
using Terraria;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Common.Interaction;
using TerrariaOverhaul.Common.ProjectileEffects;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Content.Projectiles;

public class MopProjectile : SpearProjectileBase
{
	public static ProjectileDecals.Data? DecalPreset { get; private set; }

	protected override float HoldoutRangeMin => 40f;
	protected override float HoldoutRangeMax => 80f;

	public override void Load()
	{
		DecalPreset = new() {
			Texture = Mod.Assets.Request<Texture2D>("Assets/Textures/Decals/CleaningDecal"),
			Size = new Vector2Int(48, 48),
			DecalStyle = DecalStyle.Subtractive,
			IfChunkExists = true,
		};
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.width = 16;
		Projectile.height = 16;

		if (!Main.dedServ) {
			if (Projectile.TryGetGlobalProjectile(out ProjectileDecals decals)) {
				decals.OnTick = DecalPreset;
			}

			if (Projectile.TryGetGlobalProjectile(out ProjectileGoreInteraction goreInteraction)) {
				goreInteraction.FireInteraction = ProjectileGoreInteraction.FireProperties.Extinguisher;
				goreInteraction.HitEffectMultiplier = 0.333f; // Produce less explosive blood when destroying gore.
				goreInteraction.DisableGoreHitAudio = false; // Hit gore silently.
				goreInteraction.DisableGoreHitCooldown = true; // Hit much more gore than usually.
			}
		}
	}
}
