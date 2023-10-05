using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public sealed class BloodColorRecording : ModSystem
{
	private static readonly Stack<List<Color>> recordingLists = new();
	private static readonly Stack<List<Color>> listPool = new(); // Pool to minimize array growth & GC stress.

	public override void Unload()
	{
		recordingLists?.Clear();
		listPool?.Clear();
	}

	/// <summary> Returns a list of colors of blood that has been created during execution of the provided delegate. </summary>
	public static List<Color> RecordBloodColors(Action innerAction)
	{
		if (listPool.TryPop(out var list)) {
			list.Clear();
		} else {
			list = new List<Color>();
		}

		recordingLists.Push(list);

		try {
			innerAction();
		}
		finally {
			recordingLists.TryPop(out _);
			listPool.Push(list);
		}

		return list!;
	}

	public static void AddColors(ReadOnlySpan<Color> colors)
	{
		foreach (var list in recordingLists) {
			// ffs, why is this not a thing for spans?
			//list.AddRange(colors);

			list.EnsureCapacity(list.Count + colors.Length);

			for (int i = 0; i < colors.Length; i++) {
				list.Add(colors[i]);
			}
		}
	}
}
