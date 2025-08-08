// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.Decals;

namespace TerrariaOverhaul.Common.Damage;

[Autoload(Side = ModSide.Client)]
public sealed class NPCDamageVisuals : GlobalNPC
{
	public override void HitEffect(NPC npc, NPC.HitInfo hit)
	{
		if (!npc.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore)) return;
		if (npcBloodAndGore.LastHitBloodAmount == 0) return;
		if (npcBloodAndGore.LastHitBloodColor.A == 0) return;

		const int NumFrames = 5;
		const int FrameWidth = 32;
		const int FrameHeight = 32;
		var texture = ModContent.Request<Texture2D>($"{nameof(TerrariaOverhaul)}/Assets/Textures/Decals/BloodSplatBigger", AssetRequestMode.ImmediateLoad).Value;
		var color = npcBloodAndGore.LastHitBloodColor.MultiplyRGB(new Color(192, 192, 192));

		int numRolls = npc.life <= 0 ? 3 : 1;
		int numSplats = 0;
		for (int i = 0; i < numRolls; i++)
			if (Main.rand.NextBool(3)) numSplats++;

		for (int i = 0; i < numSplats; i++) {
			int frame = Main.rand.Next(NumFrames);
			var srcRect = new Rectangle(0, frame * FrameWidth, FrameWidth, FrameHeight);
			float offsetLength = Main.rand.NextFloat(5f, 10f);
			var position = npc.Center + (npc.velocity.RotatedByRandom(MathHelper.PiOver2) * offsetLength) + Main.rand.NextVector2Circular(npc.width, npc.height);

			DecalSystem.AddDecals(DecalStyle.Default, new DecalInfo {
				Layers = DecalLayerFlags.Background,
				Texture = texture,
				SrcRect = srcRect,
				Position = position,
				Color = color,
			});
		}
	}
}
