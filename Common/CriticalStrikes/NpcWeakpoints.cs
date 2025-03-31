using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Melee;
using TerrariaOverhaul.Core.Debugging;
using TerrariaOverhaul.Core.Tags;
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
	public bool WeakpointIsMainSegment;
	public bool UsesRotationAngle;
	public bool UsesSpriteDirection = true;
	public sbyte DirectionalWeakpoint = 1;
	public Weakpoint[] Weakpoints = [];
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

	private static readonly ContentSet WeakpointUsesRotationAngle = nameof(WeakpointUsesRotationAngle);
	private static readonly ContentSet WeakpointUsesSpriteDirection = nameof(WeakpointUsesSpriteDirection);
	private static readonly ContentSet WeakpointIsMainSegment = nameof(WeakpointIsMainSegment);

	public override bool InstancePerEntity => true;

	public override void SetDefaults(NPC npc)
	{
		Array.Resize(ref weakpointsByType, NPCLoader.NPCCount);

		bool CheckSet(ContentSet set) => set.Has(npc) || set.Has<NPCAIStyleID>(npc.aiStyle);

		var info = new WeakpointInfo();
		info.UsesRotationAngle = CheckSet(WeakpointUsesRotationAngle);
		info.UsesSpriteDirection = CheckSet(WeakpointUsesSpriteDirection);
		info.WeakpointIsMainSegment = CheckSet(WeakpointIsMainSegment);

		WeakpointsByType[npc.type] = info;
	}

	public override void PostAI(NPC npc)
	{
		sbyte direction = (sbyte)npc.spriteDirection;
		if (direction != lastDirection) {
			lastDirectionSwitchTime = Main.GameUpdateCount;
			lastDirection = direction;
		}

		//npc.rotation = MathUtils.LerpRadians(npc.rotation, npc.velocity.ToRotation() - MathHelper.PiOver2, 0.75f);
	}

	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		if (!CriticalStrikeRework.EnableCriticalStrikeRework)
			return;

		var npcRect = npc.getRect();
		var corner = npcRect.GetCorner(player.Center);
		var attackDirection = npc.DirectionTo(player.Center);

		if (!player.HeldItem.IsAir && player.HeldItem.TryGetGlobalItem(out ItemMeleeAttackAiming aiming)) {
			attackDirection = aiming.AttackDirection;
		}

		if (CheckWeakpoints(npc, corner, attackDirection)) {
			using var _ = CriticalStrikeRework.AllowCritChanceReturn();
			
			int critChance = Main.LocalPlayer.GetWeaponCrit(item);
			float critMult = CriticalStrikeRework.TotalCritChanceToDamageScale(critChance);
			TriggerCrit(ref modifiers, critMult);
		}
	}

	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		if (!CriticalStrikeRework.EnableCriticalStrikeRework)
			return;

		var npcRect = npc.getRect();
		var oldProjCenter = projectile.oldPosition + projectile.Size * 0.5f;
		var corner = npcRect.GetCorner(oldProjCenter);
		var attackDirection = projectile.velocity.SafeNormalize(Vector2.UnitX);

		if (CheckWeakpoints(npc, corner, attackDirection)) {
			using var _ = CriticalStrikeRework.AllowCritChanceReturn();
			
			var ownerPlayer = projectile.GetOwner();
			int critChance = ownerPlayer?.GetWeaponCrit(ownerPlayer?.HeldItem) ?? 0;
			float critMult = CriticalStrikeRework.TotalCritChanceToDamageScale(critChance);
			TriggerCrit(ref modifiers, critMult);
		}
	}

	private bool CheckWeakpoints(NPC npc, Vector2 point, Vector2 attackDirection)
	{
		ref readonly var info = ref WeakpointsByType[npc.type];
		int npcDirection = info.UsesSpriteDirection ? npc.spriteDirection : npc.direction;
		bool justChangedDirections = JustChangedDirections;

		if (info.WeakpointIsMainSegment) {
			if (npc.realLife != -1 && npc.realLife == npc.whoAmI) {
				return true;
			}
		} else if (info.DirectionalWeakpoint != 0) {
			if (justChangedDirections) return true;

			if (!info.UsesRotationAngle) {
				int intAttackDirection = attackDirection.X > 0f ? -1 : +1;
				if (intAttackDirection != (npcDirection * info.DirectionalWeakpoint)) {
					return true;
				}
			} else {
				float radius = MathHelper.Pi;
				float halfRadius = radius * 0.5f;
				float npcAngle = (npc.rotation + MathHelper.PiOver2);
				//float npcAngle = npc.velocity.ToRotation();
				float hitAngle = (-attackDirection).ToRotation();
				float diff = ((hitAngle - npcAngle + MathHelper.Pi + MathHelper.TwoPi) % MathHelper.TwoPi) - MathHelper.Pi;
				bool result = diff < -halfRadius || diff > halfRadius;

				DebugSystem.DrawLine(npc.Center, npc.Center + (Vector2.UnitX.RotatedBy(npcAngle) * 256f), Color.White, 4);
				DebugSystem.DrawLine(npc.Center, npc.Center + (Vector2.UnitX.RotatedBy(hitAngle) * 256f), Color.Red, 4);

				if (info.DirectionalWeakpoint > 0 ? result : !result) {
					return true;
				}
			}
		}

		// Weakpoint checks are ran twice for both directions if we're within a direction change race period.
		for (int i = 0; i < (justChangedDirections ? 2 : 1); i++) {
			int checkDirection = i == 0 ? npcDirection : -npcDirection;

			foreach (var weakpoint in WeakpointsByType[npc.type].Weakpoints) {
				if (weakpoint.GetWorldRectangle(npc, checkDirection).ContainsInclusive(point))
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
