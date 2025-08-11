using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Content.Flames;

public sealed class BurnedTree : ModTree
{
	public override TreePaintingSettings TreeShaderSettings { get; } = new() {
		UseSpecialGroups = true,
		SpecialGroupMinimalHueValue = 11f / 72f,
		SpecialGroupMaximumHueValue = 0.25f,
		SpecialGroupMinimumSaturationValue = 0.88f,
		SpecialGroupMaximumSaturationValue = 1f
	};

	public override int DropWood() => ItemID.Coal;
	public override int CreateDust() => 54;
	public override bool CanDropAcorn() => false;

	public override void SetStaticDefaults()
	{
		GrowsOnTileId = [ModContent.TileType<BurnedGrass>()];
	}

	public override void SetTreeFoliageSettings(Tile tile, ref int xOffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
	{
		
	}

	private static Asset<Texture2D>? tileTextureCache, topTextureCache, branchTextureCache;
	public override Asset<Texture2D> GetTexture() => tileTextureCache ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Content/Flames/BurnedTree");
	public override Asset<Texture2D> GetTopTextures() => topTextureCache ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Content/Flames/BurnedTree_Tops");
	public override Asset<Texture2D> GetBranchTextures() => branchTextureCache ??= ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Content/Flames/BurnedTree_Branches");
}
