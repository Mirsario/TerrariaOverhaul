// Copyright (c) 2020-2026 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Bosses;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Utilities.Terraria;

namespace TerrariaOverhaul.Common.EntityEffects;

internal sealed class NpcWormEffects : GlobalNPC
{
	internal enum PartType : byte
	{
		Head,
		Body,
		Tail,
	}

	public class EffectData
	{
		public PartType Type;
		public ScreenShake SurfacingScreenShake;
		public ScreenShake UndergroundScreenShake;
		public DebrisEffects.Style DebrisStyle;

		internal bool wasUnderground;
	}

	public EffectData? Data;

	public override bool InstancePerEntity => true;

	public override void PostAI(NPC npc)
	{
		if (Data is not EffectData data || Main.dedServ) {
			return;
		}

		var center = npc.Center;
		var centerPoint = center.ToTileCoordinates();

		if (!Main.tile.TryGet(centerPoint, out var centerTile)) {
			return;
		}

		bool underground = centerTile.HasTile && Main.tileSolid[centerTile.TileType] && !Main.tileSolidTop[centerTile.TileType];
		var perceivedVelocity = npc.position - npc.oldPosition;
		float perceivedSpeed = perceivedVelocity.Length();

		if (underground != data.wasUnderground) {
			// Debris sounds & effects
			if (data.DebrisStyle.Sound.SoundPath != null) {
				DebrisEffects.CreateOrUpdateDebrisAtPosition(center + perceivedVelocity, in data.DebrisStyle);
			}

			// Debris screenshake
			float shakePower = MathHelper.Clamp(perceivedSpeed / 10f, 0.0f, 1.0f);
			ScreenShakeSystem.New(data.SurfacingScreenShake with { Power = data.SurfacingScreenShake.Power * shakePower }, center);
		}

		data.wasUnderground = underground;
	}
}
