// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Decals;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
internal sealed class GoreBaking : ModSystem
{
	public static bool[] IsGoreToBeBaked = GoreID.Sets.Factory.CreateBoolSet([
		GoreID.TreeLeaf_Normal,
		GoreID.TreeLeaf_Palm,
		GoreID.TreeLeaf_Mushroom,
		GoreID.TreeLeaf_Boreal,
		GoreID.TreeLeaf_Jungle,
		GoreID.TreeLeaf_Corruption,
		GoreID.TreeLeaf_Crimson,
		GoreID.TreeLeaf_Hallow,
		GoreID.TreeLeaf_HallowLast,
		GoreID.TreeLeaf_HallowJim,
		GoreID.TreeLeaf_HallowLastJim,
		GoreID.TreeLeaf_VanityTreeSakura,
		GoreID.TreeLeaf_GemTreeTopaz,
		GoreID.TreeLeaf_GemTreeAmethyst,
		GoreID.TreeLeaf_GemTreeSapphire,
		GoreID.TreeLeaf_GemTreeEmerald,
		GoreID.TreeLeaf_GemTreeRuby,
		GoreID.TreeLeaf_GemTreeDiamond,
		GoreID.TreeLeaf_GemTreeAmber,
		GoreID.TreeLeaf_VanityTreeYellowWillow,
		GoreID.TreeLeaf_TreeAsh,
	]);

	public override void Load()
	{
		On_Gore.Update += GoreUpdateDetour;
	}

	public override void PostSetupContent()
	{
		Array.Resize(ref IsGoreToBeBaked, GoreLoader.GoreCount);
	}

	public static bool ShouldBakeGore(OverhaulGore gore)
	{
		return gore.type >= 0 && gore.type < IsGoreToBeBaked.Length && IsGoreToBeBaked[gore.type];
	}

	public static void BakeGoreAsDecal(OverhaulGore gore)
	{
		var texture = TextureAssets.Gore[gore.type].Value;
		var frame = gore.Frame.GetSourceRectangle(texture);

		DecalSystem.AddDecals(DecalStyle.Default, new DecalInfo {
			Layers = DecalLayerFlags.Foreground,
			Texture = texture,
			SrcRect = frame,
			Position = gore.position + gore.drawOffset,
			Scale = Vector2.One * gore.scale,
			Color = Color.White,
			Rotation = gore.rotation,
		});
	}

	private static void GoreUpdateDetour(On_Gore.orig_Update orig, Gore gore)
	{
		var oldVelocity = gore.velocity;
		orig(gore);
		var newVelocity = gore.velocity;

		if (newVelocity == oldVelocity || gore is not OverhaulGore oGore || !ShouldBakeGore(oGore)) {
			return;
		}

		if ((newVelocity.X == 0f && oldVelocity.X != 0f) || ((newVelocity.Y == 0f || newVelocity.Y == -1f) && oldVelocity.Y != 0f)) {
			BakeGoreAsDecal(oGore);
			oGore.active = false;
		}
	}
}
