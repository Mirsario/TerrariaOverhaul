using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Utilities;

internal static class ContentSampleUtils
{
	public static Item GetItem(int type)
		=> TryGetItem(type, out var result) ? result : throw new InvalidOperationException($"Unable to get a sample of item '{type}'.");

	public static Projectile GetProjectile(int type)
		=> TryGetProjectile(type, out var result) ? result : throw new InvalidOperationException($"Unable to get a sample of projectile '{type}'.");

	public static bool TryGetItem(int type, [NotNullWhen(true)] out Item? item)
	{
		if (ContentSamples.ItemsByType.TryGetValue(type, out item)) {
			return true;
		}

		item = ItemLoader.GetItem(type)?.Item;

		return item != null;
	}

	public static bool TryGetProjectile(int type, [NotNullWhen(true)] out Projectile? projectile)
	{
		if (ContentSamples.ProjectilesByType.TryGetValue(type, out projectile)) {
			return true;
		}

		projectile = ProjectileLoader.GetProjectile(type)?.Projectile;

		return projectile != null;
	}
}
