using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Core.Networking;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Movement;

public sealed class PlayerDirectioning : ModPlayer
{
	public sealed class PlayerMousePositionPacket : NetPacket
	{
		public PlayerMousePositionPacket(Player player)
		{
			var modPlayer = player.GetModPlayer<PlayerDirectioning>();

			Writer.TryWriteSenderPlayer(player);
			Writer.WriteVector2(modPlayer.MouseWorld);
			Writer.WriteHalfVector2(modPlayer.LookPosition - modPlayer.MouseWorld);
		}

		public override void Read(BinaryReader reader, int sender)
		{
			if (!reader.TryReadSenderPlayer(sender, out var player) || !player.TryGetModPlayer(out PlayerDirectioning modPlayer)) {
				return;
			}

			modPlayer.MouseWorld = reader.ReadVector2();
			modPlayer.LookPosition = modPlayer.MouseWorld + reader.ReadHalfVector2();

			// Resend
			if (Main.netMode == NetmodeID.Server) {
				MultiplayerSystem.SendPacket(new PlayerMousePositionPacket(player), ignoreClient: sender);
			}
		}
	}

	[Flags]
	public enum OverrideFlags
	{
		None = 0,
		IgnoreItemAnimation = 1,
	}

	private struct Override<T>
	{
		public T Value;
		public Timer Timer;
		public OverrideFlags Flags;

		public Override(T value, Timer timer, OverrideFlags flags = 0)
		{
			Value = value;
			Timer = timer;
			Flags = flags;
		}

		public readonly bool AppliesToPlayer(Player player)
		{
			if (!Timer.Active) {
				return false;
			}

			if (!Flags.HasFlag(OverrideFlags.IgnoreItemAnimation) && player.ItemAnimationActive) {
				return false;
			}

			return true;
		}
	}

	private const int MouseWorldSyncFrequency = 12;

	private static int skipSetDirectionCounter;

	private int lastSyncHash;
	private Override<Vector2> lookPositionOverride;
	private Override<Direction1D> directionOverride;

	public Vector2 MouseWorld { get; private set; }
	public Vector2 LookPosition { get; private set; }

	public override void Load()
	{
		On_Player.HorizontalMovement += static (orig, player) => {
			orig(player);

			player.GetModPlayer<PlayerDirectioning>()?.UpdateDirection();
		};

		On_Player.ItemCheck_StartActualUse += static (On_Player.orig_ItemCheck_StartActualUse orig, Player player, Item sItem) => {
			orig(player, sItem);

			player.GetModPlayer<PlayerDirectioning>()?.UpdateDirection(ignoreItemAnim: true);
		};

		On_PlayerSleepingHelper.StartSleeping += static (On_PlayerSleepingHelper.orig_StartSleeping orig, ref PlayerSleepingHelper self, Player player, int x, int y) => {
			try {
				skipSetDirectionCounter++;

				orig(ref self, player, x, y);
			}
			finally {
				skipSetDirectionCounter--;
			}
		};

		On_PlayerSittingHelper.SitDown += static (On_PlayerSittingHelper.orig_SitDown orig, ref PlayerSittingHelper self, Player player, int x, int y) => {
			try {
				skipSetDirectionCounter++;

				orig(ref self, player, x, y);
			}
			finally {
				skipSetDirectionCounter--;
			}
		};
	}

	public override void PreUpdate()
		=> UpdateDirection();

	public override void PostUpdate()
		=> UpdateDirection();

	public override bool PreItemCheck()
	{
		UpdateDirection();

		return true;
	}

	public override void PostItemCheck()
	{
		UpdateDirection();
	}

	public void UpdateDirection(bool ignoreItemAnim = false)
	{
		if (!Main.dedServ && Main.gameMenu) {
			Player.direction = 1;

			return;
		}

		if (Player.IsLocal() && Main.hasFocus) {
			MouseWorld = Main.MouseWorld;
			LookPosition = MouseWorld;

			if (lookPositionOverride.AppliesToPlayer(Player)) {
				LookPosition = lookPositionOverride.Value;
			}

			if (Main.netMode == NetmodeID.MultiplayerClient && Main.GameUpdateCount % MouseWorldSyncFrequency == 0) {
				int syncHash = unchecked(MouseWorld.GetHashCode() + LookPosition.GetHashCode());

				if (syncHash != lastSyncHash) {
					MultiplayerSystem.SendPacket(new PlayerMousePositionPacket(Player));

					lastSyncHash = syncHash;
				}
			}
		}

		if (skipSetDirectionCounter > 0 || Player.sleeping.isSleeping || Player.sitting.isSitting) {
			return;
		}

		if (!Player.pulley && (!Player.mount.Active || Player.mount.AllowDirectionChange) && (ignoreItemAnim || Player.itemAnimation <= 1 || ICanTurnDuringItemUse.Invoke(Player.HeldItem, Player))) {
			int wantedDirection;
			
			if (directionOverride.AppliesToPlayer(Player)) {
				wantedDirection = (int)directionOverride.Value;
			} else {
				wantedDirection = MouseWorld.X >= Player.Center.X ? 1 : -1;
			}

			Player.ChangeDir(wantedDirection);
		}
	}

	public void SetDirectionOverride(Direction1D direction, uint ticks, OverrideFlags flags = OverrideFlags.None)
	{
		if (ticks != 0) {
			directionOverride = new(direction, ticks, flags);

			UpdateDirection();
		}
	}

	public void SetLookPositionOverride(Vector2 point, uint ticks, OverrideFlags flags = OverrideFlags.None)
	{
		if (ticks != 0) {
			lookPositionOverride = new(point, ticks, flags);

			UpdateDirection();
		}
	}
}
