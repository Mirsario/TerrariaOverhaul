using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities.Terraria;
using TerrariaOverhaul.Utilities.Xna;

namespace TerrariaOverhaul.Common.CriticalStrikes;

public struct Weakpoint
{
	public RectFloat Area;

	public readonly RectFloat GetWorldRectangle(NPC npc, int direction)
	{
		var area = direction > 0 ? Area : new RectFloat(
			1.0f - Area.X - Area.Width,
			Area.Y,
			Area.Width,
			Area.Height
		);
		var size = npc.Size * npc.scale;
		var result = new RectFloat(
			npc.position.X + (area.X * size.X),
			npc.position.Y + (area.Y * size.Y),
			area.Width * size.X,
			area.Height * size.Y
		);

		return result;
	}
}
public struct WeakpointInfo()
{
	public sbyte DirectionalWeakpoint;
	public Weakpoint[] Weakpoints = Array.Empty<Weakpoint>();
}

public sealed class NpcWeakpoints : GlobalNPC
{
	private const int DirectionChangeGracePeriod = 10;

	private static readonly SoundStyle WeakpointCritSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Items/Magic/MagicPowerfulBlast") {
		Pitch = 0.70f,
		PitchVariance = 0.2f,
		Volume = 0.45f,
	};

	private static WeakpointInfo[] weakpointsByType = Array.Empty<WeakpointInfo>();
	public static WeakpointInfo[] WeakpointsByType => weakpointsByType;

	private uint lastDirectionSwitchTime;
	private sbyte lastDirection;

	private bool JustChangedDirections => Main.GameUpdateCount - lastDirectionSwitchTime < DirectionChangeGracePeriod;

	public override bool InstancePerEntity => true;

	public override void SetDefaults(NPC npc)
	{
		Array.Resize(ref weakpointsByType, NPCLoader.NPCCount);

		ref var info = ref WeakpointsByType[npc.type];
		info = new();

		//if (npc.aiStyle is NPCAIStyleID.Fighter or NPCAIStyleID.Passive or NPCAIStyleID.TargetDummy or NPCAIStyleID.Caster) {
		info.DirectionalWeakpoint = 1;
		//}
	}

	public override void PostAI(NPC npc)
	{
		sbyte direction = (sbyte)npc.spriteDirection;
		if (direction != lastDirection) {
			lastDirectionSwitchTime = Main.GameUpdateCount;
			lastDirection = direction;
		}
	}

	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		if (!CriticalStrikeRework.EnableCriticalStrikeRework) {
			return;
		}

		var npcRect = npc.getRect();
		var corner = npcRect.GetCorner(player.Center);
		int attackDirection = Math.Sign(npc.DirectionTo(player.Center).X);

		if (CheckWeakpoints(npc, corner, attackDirection)) {
			using var _ = CriticalStrikeRework.AllowCritChanceReturn();
			
			int critChance = Main.LocalPlayer.GetWeaponCrit(item);
			float critMult = CriticalStrikeRework.TotalCritChanceToDamageScale(critChance);
			TriggerCrit(ref modifiers, critMult);
		}
	}

	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		if (!CriticalStrikeRework.EnableCriticalStrikeRework) {
			return;
		}

		var npcRect = npc.getRect();
		var oldProjCenter = projectile.oldPosition + projectile.Size * 0.5f;
		var corner = npcRect.GetCorner(oldProjCenter);
		int attackDirection = -Math.Sign(projectile.velocity.X);

		if (CheckWeakpoints(npc, corner, attackDirection)) {
			using var _ = CriticalStrikeRework.AllowCritChanceReturn();
			
			var ownerPlayer = projectile.GetOwner();
			int critChance = ownerPlayer?.GetWeaponCrit(ownerPlayer?.HeldItem) ?? 0;
			float critMult = CriticalStrikeRework.TotalCritChanceToDamageScale(critChance);
			TriggerCrit(ref modifiers, critMult);
		}
	}

	private bool CheckWeakpoints(NPC npc, Vector2 point, int attackDirection)
	{
		if (JustChangedDirections || attackDirection != npc.spriteDirection) {
			return true;
		}

		for (int i = 0; i < 2; i++) {
			int direction = npc.aiStyle == NPCAIStyleID.Slime ? npc.direction : npc.spriteDirection;
			if (i == 1) {
				if (Main.GameUpdateCount - lastDirectionSwitchTime < DirectionChangeGracePeriod)
					direction = -direction;
				else
					break;
			}

			foreach (var weakpoint in WeakpointsByType[npc.type].Weakpoints) {
				if (weakpoint.GetWorldRectangle(npc, direction).ContainsInclusive(point))
					return true;
			}
		}

		return false;
	}

	private static void TriggerCrit(ref NPC.HitModifiers modifiers, float multiplier)
	{
		modifiers.SetCrit();
		modifiers.FinalDamage *= 0.5f * multiplier;
		modifiers.Knockback *= 0.5f * multiplier;

		if (!Main.dedServ) {
			SoundEngine.PlaySound(in WeakpointCritSound);
		}
	}
}
