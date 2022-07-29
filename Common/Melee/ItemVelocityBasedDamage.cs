using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using TerrariaOverhaul.Common.CombatTexts;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Melee;

public sealed class ItemVelocityBasedDamage : ItemComponent
{
	private static readonly Gradient<Color> DamageScaleColor = new(
		(0f, Color.Black),
		(1f, Color.LightGray),
		(1.25f, Color.Green),
		(1.75f, Color.Yellow),
		(2.5f, Color.Red)
	);

	public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
	{
		if (!Enabled) {
			return;
		}

		float velocityDamageScale = Math.Max(1f, 0.78f + player.velocity.Length() / 8f);

		knockback *= velocityDamageScale;
		damage = (int)Math.Round(damage * velocityDamageScale);

		if (Main.dedServ) {
			return;
		}

		bool critBackup = crit;

		CombatTextSystem.AddFilter(1, text => {
			if (!uint.TryParse(text.text, out _)) {
				return;
			}

			bool isCharged = false;
			string additionalInfo = $"({(critBackup ? "CRITx" : null)}{(isCharged ? "POWERx" : critBackup ? null : "x")}{velocityDamageScale:0.00})";
			float gradientScale = velocityDamageScale;

			if (critBackup) {
				gradientScale *= 2;
			}

			if (isCharged) {
				gradientScale *= 1.3f;
			}

			var font = FontAssets.CombatText[critBackup ? 1 : 0].Value;
			var size = font.MeasureString(text.text);

			text.color = DamageScaleColor.GetValue(gradientScale);
			text.position.Y -= 16f;

			/*if(headshot) {
				text.text += "!";
			}*/

			//text.text += $"\r\n{additionalInfo}";

			CombatText.NewText(new Rectangle((int)(text.position.X + size.X * 0.5f), (int)(text.position.Y + size.Y + 4), 1, 1), text.color, additionalInfo, critBackup);
		});
	}
}
