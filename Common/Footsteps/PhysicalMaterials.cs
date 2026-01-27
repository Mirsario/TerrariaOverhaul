// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Data;
using TerrariaOverhaul.Core.Tags;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Footsteps;

internal interface IMaterialProvider
{
	Prefab MaterialPrefab { get; }
}

internal interface IContentSetAssociated
{
	public ContentSet ContentSet { get; }
}

internal struct PhysicalMaterial : IComponent
{
	public SoundStyle? HitSound;
	public ContentSet AssociatedSet;
}

internal sealed class PhysicalMaterials : ModSystem
{
	private static readonly Query physicalMaterials = Prefabs.Query().With<PhysicalMaterial>();
	private static Prefab[] tileMaterialLookup = [];
	private static Prefab[] wallMaterialLookup = [];

	public override void Unload()
	{
		tileMaterialLookup = [];
		wallMaterialLookup = [];
	}

	public override void PostSetupContent()
	{
		Array.Resize(ref tileMaterialLookup, TileLoader.TileCount);
		Array.Resize(ref wallMaterialLookup, WallLoader.WallCount);

		static void FillPrefabLookup(Span<Prefab> lookup, ReadOnlySpan<BitMask<ulong>> masks, Prefab prefab)
		{
			for (int maskIndex = 0; maskIndex < masks.Length; maskIndex++) {
				int baseIndex = maskIndex * BitMask<ulong>.BitSize;

				foreach (int bitIndex in masks[maskIndex]) {
					int index = baseIndex + bitIndex;
					if (index < lookup.Length) {
						lookup[index] = prefab;
					}
				}
			}
		}

		foreach (var prefab in physicalMaterials) {
			ref readonly var material = ref prefab.Get<PhysicalMaterial>();

			if (material.AssociatedSet is ContentSet set) {
				FillPrefabLookup(tileMaterialLookup, set.Values<TileID>(), prefab);
				FillPrefabLookup(wallMaterialLookup, set.Values<WallID>(), prefab);
			}
		}
	}

	public static bool TryGetTileMaterial(ushort type, out Prefab result)
	{
		ModContent.GetInstance<PhysicalMaterials>().PostSetupContent();
		result = tileMaterialLookup[type];
		return result.IsValid;
	}

	public static bool TryGetWallMaterial(ushort type, out Prefab result)
	{
		result = wallMaterialLookup[type];
		return result.IsValid;
	}
}
