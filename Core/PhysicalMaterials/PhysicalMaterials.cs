// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Tags;

namespace TerrariaOverhaul.Core.PhysicalMaterials;

public interface IPhysicalMaterialProvider
{
	PhysicalMaterial? PhysicalMaterial { get; }
}
public interface IContentSetAssociated
{
	public ContentSet ContentSet { get; }
}

public abstract class PhysicalMaterial : ModType
{
	public virtual SoundStyle? HitSound => null;

	protected sealed override void Register()
	{
		PhysicalMaterialSystem.AddPhysicalMaterial(this);
	}
}

public sealed class PhysicalMaterialSystem : ModSystem
{
	private static readonly List<PhysicalMaterial> types = new();
	public static ReadOnlySpan<PhysicalMaterial> Types => CollectionsMarshal.AsSpan(types);

	public override void Unload()
	{
		types.Clear();
	}

	public static bool TryGetTilePhysicalMaterial(ushort type, out PhysicalMaterial result)
	{
		foreach (var material in types) {
			if (material is IContentSetAssociated tagAssociated && tagAssociated.ContentSet.HasTile(type)) {
				result = material;

				return true;
			}
		}

		result = default!;

		return false;
	}

	public static bool TryGetWallPhysicalMaterial(ushort type, out PhysicalMaterial result)
	{
		foreach (var material in types) {
			if (material is IContentSetAssociated tagAssociated && tagAssociated.ContentSet.HasWall(type)) {
				result = material;

				return true;
			}
		}

		result = default!;

		return false;
	}

	internal static void AddPhysicalMaterial(PhysicalMaterial material)
	{
		types.Add(material);
	}
}
