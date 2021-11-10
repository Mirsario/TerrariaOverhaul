using System;
using System.Reflection;
using Terraria;

namespace TerrariaOverhaul.Common.Systems.Seasons.Components
{
	//Replaces rain with snow
	public sealed class SnowSeasonComponent : SeasonComponent
	{
		private static Action<SceneMetrics, int> snowTileCountSetter;

		public override void Load()
		{
			base.Load();

			snowTileCountSetter = (Action<SceneMetrics, int>)typeof(SceneMetrics)
				.GetProperty(nameof(SceneMetrics.SnowTileCount), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
				.SetMethod
				.CreateDelegate(typeof(Action<SceneMetrics, int>));

			On.Terraria.Main.snowing += orig => WrapCall(() => orig());

			On.Terraria.Rain.NewRain += (orig, position, velocity) => {
				int result = 0;

				WrapCall(() => {
					result = orig(position, velocity);
				});

				return result;
			};
		}

		public override void Unload()
		{
			snowTileCountSetter = null;
		}

		private static void WrapCall(Action action)
		{
			if (!SeasonSystem.CurrentSeason.Components.Has<SnowSeasonComponent>() || snowTileCountSetter == null) {
				action();
				return;
			}

			var sceneMetrics = Main.SceneMetrics;
			int originalSnowCount = sceneMetrics.SnowTileCount;

			try {
				snowTileCountSetter(sceneMetrics, SceneMetrics.SnowTileMax);
				action();
			}
			finally {
				snowTileCountSetter(sceneMetrics, originalSnowCount);
			}
		}
	}
}
