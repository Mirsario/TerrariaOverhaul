using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Content.Projectiles;

public class MopProjectile : SpearProjectileBase
{
	private static Asset<Texture2D>? clearingDecalTexture;

	protected override float HoldoutRangeMin => 40f;
	protected override float HoldoutRangeMax => 80f;

	public override void Load()
	{
		clearingDecalTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/CleaningDecal");
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.width = 16;
		Projectile.height = 16;

		if (!Main.dedServ && Projectile.TryGetGlobalProjectile(out ProjectileGoreInteraction goreInteraction)) {
			goreInteraction.FireInteraction = ProjectileGoreInteraction.FireProperties.Extinguisher;
			goreInteraction.HitEffectMultiplier = 0.333f; // Produce less explosive blood when destroying gore.
			goreInteraction.DisableGoreHitAudio = false; // Hit gore silently.
			goreInteraction.DisableGoreHitCooldown = true; // Hit much more gore than usually.
		}
	}

	public override void PostAI()
	{
		base.PostAI();

		if (Main.dedServ || clearingDecalTexture?.IsLoaded != true) {
			return;
		}

		var center = (Vector2Int)Projectile.Center;
		var clearSize = new Vector2Int(48, 48);
		var clearRectangle = new Rectangle(
			center.X - clearSize.X / 2,
			center.Y - clearSize.Y / 2,
			clearSize.X,
			clearSize.Y
		);

		DecalSystem.ClearDecals(clearingDecalTexture.Value, clearRectangle, Color.White);
	}
}
