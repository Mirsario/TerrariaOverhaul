using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Systems.Camera
{
	public sealed class CameraPlayer : ModPlayer
	{
		public override void ModifyScreenPosition() => ModContent.GetInstance<CameraSystem>().ModifyScreenPosition(player);
	}
}
