using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Systems.Gores
{
	[Autoload(Side = ModSide.Client)]
	public class GoreSystem : ModSystem
	{
		private static int disableGoreSubscriptions;
		private static List<List<(Gore gore, int index)>> goreRecordingLists;

		public override void Load()
		{
			goreRecordingLists = new List<List<(Gore,int)>>();

			On.Terraria.Gore.Update += GoreUpdate;

			On.Terraria.Gore.NewGore += (orig, position, velocity, type, scale) => {
				//Disable gore spawn, if requested.
				if(disableGoreSubscriptions > 0) {
					return Main.maxGore;
				}
				
				int result = orig(position, velocity, type, scale);

				//Convert gores to a new class.
				var goreExt = ConvertGore(Main.gore[result], () => result);

				//Record gores, if requested.
				for(int i = 0; i < goreRecordingLists.Count; i++) {
					goreRecordingLists[i].Add((goreExt, result));
				}

				return result;
			};
		}
		public override void Unload()
		{
			//Reset gores so that they don't remain of GoreExt type.
			for(int i = 0; i < Main.gore.Length; i++) {
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
		private static void GoreUpdate(On.Terraria.Gore.orig_Update orig, Gore gore)
		{
			orig(gore);

			if(!gore.active || gore.type == 0) {
				return;
			}

			if(!(gore is OverhaulGore goreExt)) {
				goreExt = ConvertGore(gore, () => Array.IndexOf(Main.gore, gore)); //TODO: Avoid this IndexOf call?
			}

			goreExt.PostUpdate();
		}
	}
}
