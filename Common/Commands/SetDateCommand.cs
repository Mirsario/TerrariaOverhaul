﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Systems.Seasons;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Commands
{
	public sealed class SetDateCommand : ModCommand
	{
		public override string Command => "oSetDate";
		public override string Description => "Sets the seasonal day to the provided integer.";
		public override CommandType Type => CommandType.World;

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if(args.Length != 1 || !int.TryParse(args[0], out int dayNum) || dayNum < 0) {
				MessageUtils.NewText("Expected 1 non-negative integer.");
				return;
			}

			SeasonSystem.SetDate(dayNum, true);
		}
	}
}
