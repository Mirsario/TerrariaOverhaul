using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public class GoreSystem : ModSystem
{
	private static readonly List<List<(Gore gore, int index)>> goreRecordingLists = new();
	
	private static int disableGoreSubscriptions;

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

	/// <summary> Invokes the provided delegate. During its execution, spawning of gores will not do anything. </summary>
	public static void InvokeWithGoreSpawnDisabled(Action action)
	{
		disableGoreSubscriptions++;

		try {
			action();
		}
		finally {
			disableGoreSubscriptions--;
		}
	}
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
		orig(gore);

		if (!gore.active || gore.type == 0) {
			return;
		}

		if (gore is not OverhaulGore goreExt) {
			goreExt = ConvertGore(gore, () => Array.IndexOf(Main.gore, gore)); //TODO: Avoid this IndexOf call?
		}
		goreExt.PostUpdate();
	}

	private static int NewGoreDetour(On_Gore.orig_NewGore_IEntitySource_Vector2_Vector2_int_float orig, IEntitySource entitySource, Vector2 position, Vector2 velocity, int type, float scale)
	{
		// Disable gore spawn, if requested.
		if (disableGoreSubscriptions > 0) {
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
