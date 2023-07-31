using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace TerrariaOverhaul.Common.Archery;

public static class ArcheryWeapons
{
	public enum Kind
	{
		Undefined,
		Bow,
		Repeater,
	}

	public static readonly SoundStyle FireSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowFire", 4) {
		Volume = 0.375f,
		PitchVariance = 0.2f,
		MaxInstances = 3,
	};
	public static readonly SoundStyle ChargeSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowCharge", 4) {
		Volume = 0.375f,
		PitchVariance = 0.2f,
		MaxInstances = 3,
	};
	public static readonly SoundStyle EmptySound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Bows/BowEmpty") {
		Volume = 0.375f,
		PitchVariance = 0.2f,
		MaxInstances = 3,
	};


	public static bool IsArcheryWeapon(Item item, out Kind kind)
	{
		kind = Kind.Undefined;

		// Ignore weapons that don't shoot, and ones that deal hitbox damage 
		if (item.shoot <= ProjectileID.None || !item.noMelee) {
			return false;
		}

		// Ignore weapons that don't shoot arrows.
		if (item.useAmmo != AmmoID.Arrow) {
			return false;
		}

		// Avoid tools and placeables
		if (item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.createTile >= TileID.Dirt || item.createWall >= 0) {
			return false;
		}

		kind = GetKind(item);

		return true;
	}

	private static Kind GetKind(Item item)
	{
		if (item.width >= item.height) {
			return Kind.Repeater;
		}

		return Kind.Bow;
	}
}
