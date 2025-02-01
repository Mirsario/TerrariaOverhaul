// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.PhysicalMaterials;

public abstract class PhysicalMaterial : ModType
{
	public virtual SoundStyle? HitSound => null;

	protected sealed override void Register()
	{
		PhysicalMaterialSystem.AddPhysicalMaterial(this);
	}
}
