using System.Collections.Generic;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.PhysicalMaterials
{
	public sealed class PhysicalMaterialSystem : ModSystem
	{
		public static readonly IReadOnlyList<PhysicalMaterial> PhysicalMaterials;

		private static readonly List<PhysicalMaterial> PhysicalMaterialsInternal;

		static PhysicalMaterialSystem()
		{
			PhysicalMaterials = (PhysicalMaterialsInternal = new()).AsReadOnly();
		}

		public override void Unload()
		{
			PhysicalMaterialsInternal.Clear();
		}

		public static bool TryGetTilePhysicalMaterial(int type, out PhysicalMaterial result)
		{
			foreach (var material in PhysicalMaterialsInternal) {
				if (material is ITileTagAssociated tagAssociated && tagAssociated.TileTag.Has(type)) {
					result = material;

					return true;
				}
			}

			result = default!;

			return false;
		}

		public static bool TryGetWallPhysicalMaterial(int type, out PhysicalMaterial result)
		{
			foreach (var material in PhysicalMaterialsInternal) {
				if (material is IWallTagAssociated tagAssociated && tagAssociated.WallTag.Has(type)) {
					result = material;

					return true;
				}
			}

			result = default!;

			return false;
		}

		internal static void AddPhysicalMaterial(PhysicalMaterial material)
		{
			PhysicalMaterialsInternal.Add(material);
		}
	}
}
