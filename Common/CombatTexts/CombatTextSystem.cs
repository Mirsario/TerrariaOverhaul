using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Time;

namespace TerrariaOverhaul.Common.CombatTexts
{
	public sealed class CombatTextSystem : ModSystem
	{
		private struct Filter
		{
			public Action<CombatText> action;
			public ulong removeAt;
		}

		private static List<Filter> filters;
		private static bool skip;

		public override void Load()
		{
			filters = new List<Filter>();

			On.Terraria.CombatText.NewText_Rectangle_Color_string_bool_bool += (orig, location, color, text, dramatic, dot) => {
				int result = orig(location, color, text, dramatic, dot);

				if (!skip) {
					var combatText = Main.combatText.IndexInRange(result) ? Main.combatText[result] : null;

					if (combatText != null) {
						try {
							skip = true;

							for (int i = 0; i < filters.Count; i++) {
								var action = filters[i];

								if (action.removeAt <= TimeSystem.UpdateCount) {
									filters.RemoveAt(i--);
									continue;
								}

								action.action(combatText);
							}
						}
						finally {
							skip = false;
						}
					}
				}

				return result;
			};
		}

		public override void Unload()
		{
			if (filters != null) {
				filters.Clear();

				filters = null;
			}
		}

		public static void AddFilter(int lifeTime, Action<CombatText> action)
		{
			filters.Add(new Filter {
				action = action,
				removeAt = TimeSystem.UpdateCount + (ulong)lifeTime
			});
		}
	}
}
