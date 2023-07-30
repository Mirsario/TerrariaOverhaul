using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Damage;

public class NPCAttackCooldowns : GlobalNPC
{
	private Color? defaultColor;
	private Timer cooldownPenaltyTimer;

	public uint AttackCooldown { get; private set; }
	public uint CooldownPenalty { get; private set; }
	public bool ShowDamagedEffect { get; private set; }

	public override bool InstancePerEntity => true;

	public override bool PreAI(NPC npc)
	{
		if (!cooldownPenaltyTimer.Active && CooldownPenalty != 0) {
			//Main.NewText($"{npc.TypeName}: {CooldownPenalty} -> 0", Color.MediumVioletRed);
			CooldownPenalty = 0;
		}

		if (AttackCooldown != 0 && --AttackCooldown == 0 && ShowDamagedEffect) {
			npc.color = defaultColor ?? default;
			ShowDamagedEffect = false;
		}

		return true;
	}

	public override bool CanHitNPC(NPC npc, NPC target)
		=> AttackCooldown == 0;

	public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		=> AttackCooldown == 0;

	public void SetAttackCooldown(NPC npc, uint ticks, bool isDamage)
	{
		if (ticks == 0) {
			return;
		}

		uint reducedTicks = (uint)Math.Max(0, ticks - (int)CooldownPenalty);

		if (reducedTicks != 0 || AttackCooldown != 0) {
			if (reducedTicks > AttackCooldown) {
				AttackCooldown = reducedTicks;
				//Main.NewText($"{npc.TypeName}: {AttackCooldown}", Color.Aquamarine);
			}

			if (isDamage) {
				const float ColorBlend = 0.5f;

				defaultColor ??= npc.color;
				npc.color = Color.Lerp(npc.color, new Color(0.9f, 0.0f, 0.0f), ColorBlend);
				ShowDamagedEffect = true;
			}
		}

		const int PenaltyStart = 2;
		const int PenaltyAccumulation = 2;
		const float PenaltyResetTimeInSeconds = 0.75f;

		uint previousCooldownPenalty = CooldownPenalty;

		CooldownPenalty = CooldownPenalty == 0 ? PenaltyStart : CooldownPenalty + PenaltyAccumulation;

		//Main.NewText($"{npc.TypeName}: {previousCooldownPenalty} -> {CooldownPenalty}", Color.Gold);

		cooldownPenaltyTimer.Set((uint)(PenaltyResetTimeInSeconds * TimeSystem.LogicFramerate));
	}
}
