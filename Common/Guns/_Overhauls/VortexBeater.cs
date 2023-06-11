using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Crosshairs;
using TerrariaOverhaul.Common.Recoil;
using TerrariaOverhaul.Content.Gores;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.ItemOverhauls;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Guns;

[ItemAttachment(ItemID.VortexBeater)]
public sealed class VortexBeater : ItemOverhaul
{
	public override void Load()
	{
		base.Load();

		IL_Projectile.AI_075 += HeldProjectileBehaviorInjection;
	}

	public override bool ShouldApplyItemOverhaul(Item item)
		=> false;

	public override void SetDefaults(Item item)
	{
		base.SetDefaults(item);

		item.UseSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Guns/AssaultRifle/AssaultRifleFire", 3) {
			Volume = 0.130f,
			Pitch = 0.6f,
			PitchVariance = 0.0f,
			MaxInstances = 1,
		};

		if (!Main.dedServ) {
			item.EnableComponent<ItemAimRecoil>();
			item.EnableComponent<ItemCrosshairController>(c => {
				c.UseItemEffects = new CrosshairEffects {
					Offset = (7.0f, 0.25f),
					InnerColor = (Color.White, 0.125f),
				};
				c.UseAnimationEffects = null;
			});

			item.EnableComponent<ItemUseScreenShake>();

			item.EnableComponent<ItemBulletCasings>(c => {
				c.CasingGoreType = ModContent.GoreType<BulletCasing>();
			});
		}
	}

	private static void HeldProjectileBehaviorInjection(ILContext context)
	{
		var il = new ILCursor(context);

		// Match the first 'if (type == 615)'.
		il.GotoNext(
			MoveType.After,
			i => i.MatchLdarg(0),
			i => i.MatchLdfld(typeof(Projectile), nameof(Projectile.type)),
			i => i.MatchLdcI4(ProjectileID.VortexBeater),
			i => i.MatchBneUn(out _)
		);

		// Match 'if (ai[1] <= 0f)', which is the block for triggering firing.
		il.GotoNext(
			MoveType.After,
			i => i.MatchLdarg(0),
			i => i.MatchLdfld(typeof(Projectile), nameof(Projectile.ai)),
			i => i.MatchLdcI4(1),
			i => i.MatchLdelemR4(),
			i => i.MatchLdcR4(0f),
			i => i.MatchBgtUn(out _)
		);

		int weaponFiringEmitIndex = il.Index;

		// Match the next 'if (soundDelay <= 0)'.
		ILLabel? skipSoundPlayLabel = null;

		il.GotoNext(
			MoveType.After,
			i => i.MatchLdarg(0),
			i => i.MatchLdfld(typeof(Projectile), nameof(Projectile.soundDelay)),
			i => i.MatchLdcI4(0),
			i => i.MatchBgt(out skipSoundPlayLabel)
		);

		// Emit sound skip
		il.Emit(OpCodes.Ldarg_0);
		il.EmitDelegate(ShouldSkipFiringSound);
		il.Emit(OpCodes.Brtrue, skipSoundPlayLabel!);

		// Emit weapon firing code.
		il.Index = weaponFiringEmitIndex;

		il.Emit(OpCodes.Ldarg_0);
		il.EmitDelegate(OnWeaponFiring);
	}

	private static bool ShouldSkipFiringSound(Projectile projectile)
	{
		return true;
	}

	private static void OnWeaponFiring(Projectile projectile)
	{
		if (projectile.GetOwner() is not Player player) {
			return;
		}

		if (player.HeldItem is not Item { IsAir: false } item) {
			return;
		}

		// Simulate item use
		ItemLoader.UseItem(item, player);

		// Play sound
		SoundEngine.PlaySound(item.UseSound, player.Center);
	}
}
