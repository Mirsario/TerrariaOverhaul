using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Banners;

public sealed class BannerReworkSystem : ModSystem
{
	private static bool doubledLoot;
	private static bool bannerDamageDisabled;
	private static ItemID.BannerEffect[]? defaultBannerEffects;

	public static readonly ConfigEntry<bool> BannerReworkEnabled = new(ConfigSide.Both, "Banners", nameof(BannerReworkEnabled), () => true);

	public override void Load()
	{
		On.Terraria.NPC.NPCLoot_DropItems += (orig, npc, closestPlayer) => {
			orig(npc, closestPlayer);

			if (ShouldDoubleLoot(npc)) {
				orig(npc, closestPlayer);
			}
		};
		
		On.Terraria.NPC.NPCLoot_DropMoney += (orig, npc, closestPlayer) => {
			orig(npc, closestPlayer);

			if (ShouldDoubleLoot(npc)) {
				orig(npc, closestPlayer);
			}
		};
	}

	public override void PreUpdateWorld()
	{
		bool enable = BannerReworkEnabled.Value;

		if (enable != bannerDamageDisabled) {
			if (enable) {
				if (defaultBannerEffects == null) {
					defaultBannerEffects = ItemID.Sets.BannerStrength;
				}

				ItemID.Sets.BannerStrength = ItemID.Sets.Factory.CreateCustomSet(new ItemID.BannerEffect(1f, 1f, 1f, 1f));
			} else {
				ItemID.Sets.BannerStrength = defaultBannerEffects ?? throw new Exception($"{nameof(BannerReworkSystem)} failed miserably!");
			}

			bannerDamageDisabled = enable;
		}
	}

	private static bool ShouldDoubleLoot(NPC npc)
	{
		if (npc.type < NPCID.None || !BannerReworkEnabled) {
			return false;
		}
		
		int bannerId = Item.NPCtoBanner(npc.BannerID());

		if (bannerId <= 0) {
			return false;
		}
		
		const float MinDistanceSquared = 2048f * 2048f;

		Vector2 npcCenter = npc.Center;

		foreach (var player in ActiveEntities.Players) {
			if (player.HasNPCBannerBuff(bannerId) && Vector2.DistanceSquared(player.Center, npcCenter) < MinDistanceSquared) {
				return true;
			}
		}

		return false;
	}
}
