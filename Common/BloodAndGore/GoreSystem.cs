// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
internal class GoreSystem : ModSystem
{
	private static readonly List<List<(Gore gore, int index)>> goreRecordingLists = new();
	
	private static Counter disableGoreCounter;

	public override void Load()
	{
		// A few tweaks to vanilla gores:

		GoreID.Sets.DisappearSpeed[GoreID.KingSlimeCrown] = 15;
		GoreID.Sets.DisappearSpeed[GoreID.KingSlimePetCrown] = 5;
		GoreID.Sets.DisappearSpeedAlpha[GoreID.KingSlimeCrown] = 15;
		GoreID.Sets.DisappearSpeedAlpha[GoreID.KingSlimePetCrown] = 5;

		// IL

		On_Gore.Update += GoreUpdate;

		On_Gore.NewGore_IEntitySource_Vector2_Vector2_int_float += NewGoreDetour;
	}

	public override void Unload()
	{
		// Reset gores so that they don't remain of GoreExt type.
		for (int i = 0; i < Main.gore.Length; i++) {
			Main.gore[i] = new Gore();
		}
	}

	/// <summary> Temporarily disables gore spawn. The returned handle has to be disposed. </summary>
	public static Counter.Handle DisableGoreSpawn()
		=> disableGoreCounter.Increase();

	/// <summary> Invokes the provided delegate. Returns a list of gores that were spawned during the delegate's execution. </summary>
	public static List<(Gore gore, int goreIndex)> InvokeWithGoreSpawnRecording(Action action)
	{
		var list = new List<(Gore, int)>();

		goreRecordingLists.Add(list);

		try {
			action();
		}
		finally {
			goreRecordingLists.Remove(list);
		}

		return list;
	}

	private static OverhaulGore ConvertGore(Gore gore, Func<int> goreIndexGetter)
	{
		var result = new OverhaulGore();

		result.CopyFrom(gore);
		result.Init();

		Main.gore[goreIndexGetter()] = result;

		return result;
	}

	private static void GoreUpdate(On_Gore.orig_Update orig, Gore gore)
	{
		bool wasActive = gore.active;

		orig(gore);

		// OverhaulGore.PostUpdate() is still called on gores that just became inactive.
		if (gore.type <= 0 || !wasActive) {
			return;
		}

		if (gore is not OverhaulGore goreExt) {
			if (!gore.active) {
				return;
			}

			goreExt = ConvertGore(gore, () => Array.IndexOf(Main.gore, gore)); //TODO: Avoid this IndexOf call?
		}

		goreExt.PostUpdate();
	}

	private static int NewGoreDetour(On_Gore.orig_NewGore_IEntitySource_Vector2_Vector2_int_float orig, IEntitySource entitySource, Vector2 position, Vector2 velocity, int type, float scale)
	{
		// Disable gore spawn, if requested.
		if (disableGoreCounter.Active) {
			return Main.maxGore;
		}

		int result = orig(entitySource, position, velocity, type, scale);

		if (result < Main.maxGore) {
			// Convert gores to a new class.
			var goreExt = ConvertGore(Main.gore[result], () => result);

			// Record gores, if requested.
			for (int i = 0; i < goreRecordingLists.Count; i++) {
				goreRecordingLists[i].Add((goreExt, result));
			}
		}

		return result;
	}
}
