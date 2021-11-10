using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.ModEntities.Items.Components.Melee
{
	public sealed class ItemKillingBlows : ItemComponent
	{
		private delegate void NPCDamageModifier(NPC npc, ref double damage);

		public static readonly ISoundStyle KillingBlowSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Melee/KillingBlow", 2, volume: 0.6f, pitchVariance: 0.1f);

		[ThreadStatic]
		private static bool tryApplyingKillingBlow;

		public override void Load()
		{
			// This IL edit implements killing blows. They have to run after all damage modifications, as it needs to check the final damage.
			IL.Terraria.NPC.StrikeNPC += context => {
				var cursor = new ILCursor(context);

				int damageLocalId = 0;

				cursor.GotoNext(
					MoveType.After,
					//	num *= (double)takenDamageMultiplier;
					i => i.MatchLdloc(out damageLocalId),
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(NPC), nameof(NPC.takenDamageMultiplier)),
					i => i.Match(OpCodes.Conv_R8),
					i => i.Match(OpCodes.Mul),
					//	{
					i => i.MatchStloc(out _)
				);

				var incomingLabels = cursor.IncomingLabels.ToArray();

				cursor.Emit(OpCodes.Ldarg_0);

				foreach (var incomingLabel in incomingLabels) {
					incomingLabel.Target = cursor.Prev;
				}

				cursor.Emit(OpCodes.Ldloca, damageLocalId);
				cursor.EmitDelegate<NPCDamageModifier>(CheckForKillingBlow);
			};
		}

		public override void ModifyHitNPC(Item item, Player player, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (!Enabled) {
				return;
			}

			if (item.TryGetGlobalItem<ItemPowerAttacks>(out var powerAttacks) && !powerAttacks.PowerAttack) {
				return;
			}

			tryApplyingKillingBlow = true;
		}

		private static void CheckForKillingBlow(NPC npc, ref double damage)
		{
			if (!tryApplyingKillingBlow) {
				return;
			}

			const double Multiplier = 1.5;

			if (damage >= 0 && npc.life - damage * Multiplier <= 0.0d) {
				damage *= Multiplier;

				if (!Main.dedServ) {
					SoundEngine.PlaySound(KillingBlowSound, npc.Center);
					CombatText.NewText(npc.getRect(), Color.MediumVioletRed, "Killing Blow!", true);
				}
			}

			tryApplyingKillingBlow = false;
		}
	}
}
