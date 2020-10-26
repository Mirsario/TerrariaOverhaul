using Terraria;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Systems.Gores
{
	[Autoload(Side = ModSide.Client)]
	public class GoreSystem : ModSystem
	{
		public override void Load() => On.Terraria.Gore.Update += GoreUpdate;
		public override void Unload()
		{
			//Reset gores so that they don't remain of GoreExt type.
			for(int i = 0; i < Main.gore.Length; i++) {
				Main.gore[i] = new Gore();
			}
		}

		private static void GoreUpdate(On.Terraria.Gore.orig_Update orig, Gore gore)
		{
			orig(gore);

			if(!gore.active || gore.type == 0) {
				return;
			}

			if(!(gore is OverhaulGore goreExt)) {
				Main.gore[System.Array.IndexOf(Main.gore, gore)] = goreExt = new OverhaulGore();

				goreExt.CopyFrom(gore);
				goreExt.Init();
			}

			goreExt.PostUpdate();
		}
	}
}
