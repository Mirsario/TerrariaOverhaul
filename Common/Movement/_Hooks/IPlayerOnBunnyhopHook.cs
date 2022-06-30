using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
/*
using Hook = TerrariaOverhaul.Common.Movement.IPlayerOnBunnyhopHook;

namespace TerrariaOverhaul.Common.Movement
{
	public interface IPlayerOnBunnyhopHook
	{
		public static readonly HookList<ModPlayer> Hook = PlayerLoader.AddModHook(new HookList<ModPlayer>(typeof(Hook).GetMethod(nameof(OnBunnyhop))));

		void OnBunnyhop(Player player, ref float boost);

		public static void Invoke(Player player, ref float boost)
		{
			foreach (Hook g in Hook.Enumerate(player)) {
				g.OnBunnyhop(player, ref boost);
			}
		}
	}
}
*/
