using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Charging;
using TerrariaOverhaul.Common.Items;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Common.EntityEffects;
using TerrariaOverhaul.Core.ItemComponents;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Archery;

//TODO: Disallow hovering few pixels above the ground.
public sealed class ItemPowerAttackHover : ItemComponent
{
	private ref struct Context
	{
		public Item Item;
		public Player Player;
		public ItemPowerAttacks PowerAttacks;
		public PlayerMovement Movement;
		public ulong UpdateCount;
	}

	private static readonly Gradient<float> activationGradient = new(stackalloc Gradient<float>.Key[] {
		(0.00f, 0.0f),
		(0.25f, 1.0f),
		(0.90f, 1.0f),
		(1.00f, 0.0f),
	});

	public bool ControlsVelocityRecoil;
	public Vector4? ActivationVelocityRange = null;
	public MovementModifier Modifier = new() {
		RunAccelerationScale = 0.50f,
		VelocityScale = (Positive: new Vector2(1.00f, 0.10f), Negative: Vector2.One),
	};
	public uint InputsGracePeriod = 10;
	public uint RecoilGracePeriod = 3;

	private bool active;
	private bool needsGroundReset;
	private uint graceTicks;
	private ulong lastActiveTime;

	public override void HoldItem(Item item, Player player)
	{
		if (!Enabled) {
			return;
		}

		var c = new Context {
			Item = item,
			Player = player,
			UpdateCount = TimeSystem.UpdateCount,
		};

		// Acquire necessary components.
		if (!item.TryGetGlobalItem(out c.PowerAttacks) || !player.TryGetModPlayer(out c.Movement)) {
			return;
		}

		// Check conditions & run activation/deactivation.
		active = CheckConditions(in c);

		if (active) {
			lastActiveTime = c.UpdateCount;

			// Execute hovering
			Hover(in c);
		}

		if (ControlsVelocityRecoil && c.Item.TryGetGlobalItem(out ItemUseVelocityRecoil velocityRecoil)) {
			bool velocityRecoilActive = c.UpdateCount - lastActiveTime <= InputsGracePeriod;

			velocityRecoil.SetEnabled(c.Item, velocityRecoilActive);
		}
	}

	private bool CheckConditions(in Context c)
	{
		// Player has to be mid-air.
		if (c.Player.OnGround()) {
			needsGroundReset = false;
		}

		// Must be charging a power attack.
		if (!c.PowerAttacks.Enabled || !c.PowerAttacks.IsCharging) {
			if (active) {
				needsGroundReset = true;
			}

			graceTicks = 0;
			return false;
		}

		// If outside required velocity range when starting - cease.
		if (!active && ActivationVelocityRange is Vector4 range && c.Player.velocity is Vector2 velocity) {
			if (velocity.X < range.X || velocity.X > range.Z || velocity.Y < range.Y || velocity.Y > range.W) {
				return false;
			}
		}

		if (graceTicks > InputsGracePeriod) {
			if (!c.Player.OnGround()) {
				needsGroundReset = true;
			}

			return false;
		}

		if (!CheckGraceableConditions(in c)) {
			graceTicks++;
			return false;
		}

		graceTicks = 0;

		return true;
	}

	private bool CheckGraceableConditions(in Context c)
	{
		// Player has to be mid-air.
		if (c.Player.OnGround()) {
			needsGroundReset = false;
			return false;
		}

		// Must not be required to land
		if (needsGroundReset) {
			return false;
		}

		// Player has to be holding the jump button and not be holding down.
		if (!c.Player.controlJump || c.Player.controlDown) {
			return false;
		}

		return true;
	}

	private void Hover(in Context c)
	{
		float progressFactor = activationGradient.GetValue(c.PowerAttacks.Charge.Progress);
		float factor = progressFactor;

		var modifier = MovementModifier.Lerp(in MovementModifier.Default, in Modifier, factor);

		if (c.Player.velocity.Y >= -0.5f) {
			modifier.GravityScale *= 0f;
		}

		c.Movement.SetMovementModifier(nameof(ItemPowerAttackHover), 2, modifier);

		if (!Main.dedServ && c.Player.TryGetModPlayer(out PlayerTrailEffects trailEffects)) {
			trailEffects.ForceTrailEffect(3);
		}
	}
}
