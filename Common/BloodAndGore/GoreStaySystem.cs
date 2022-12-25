using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.BloodAndGore;

[Autoload(Side = ModSide.Client)]
public class GoreStaySystem : ModSystem
{
	public static readonly ConfigEntry<bool> EnableGoreStay = new(ConfigSide.ClientOnly, "BloodAndGore", nameof(EnableGoreStay), () => true);

	public override void Load()
	{
		IL.Terraria.Gore.NewGore_IEntitySource_Vector2_Vector2_int_float += NewGoreInjection;
		IL.Terraria.Gore.Update += GoreUpdateInjection;
	}

	// Modifies gore creation to replace a random gore slot whenever there's no free ones left.
	private static void NewGoreInjection(ILContext context)
	{
		var il = new ILCursor(context);
		bool debugAssembly = OverhaulMod.TMLAssembly.IsDebugAssembly();

		// Match 'if (num == 600)'.

		int goreIndexLocalId = 0;
		ILLabel? skipReturnLabel = null;

		if (!debugAssembly) {
			il.GotoNext(
				MoveType.After,
				i => i.MatchLdloc(out goreIndexLocalId),
				i => i.MatchLdcI4(Main.maxGore),
				i => i.MatchBneUn(out skipReturnLabel)
			);
		} else {
			il.GotoNext(
				MoveType.After,
				i => i.MatchLdloc(out goreIndexLocalId),
				i => i.MatchLdcI4(Main.maxGore),
				i => i.MatchCeq(),
				i => i.MatchStloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchBrfalse(out skipReturnLabel)
			);
		}

		// Emit replacement code.

		il.Emit(OpCodes.Ldloca, goreIndexLocalId);
		il.EmitDelegate(FindGoreSlotToReplace);
		// If a replacement is found - proceed like 'num' wasn't '600' after all.
		il.Emit(OpCodes.Brtrue, skipReturnLabel);
	}

	private static bool FindGoreSlotToReplace(ref int slot)
	{
		// Just replace a random slot!
		// Could be improved with choosing a random index of the first half of oldest-to-newest ordered gores.
		slot = Main.rand.Next(Main.maxGore);

		return true;
	}

	// Replaces all reads of 'DisappearSpeed*' sets' entries with calls of our functions.
	private static void GoreUpdateInjection(ILContext context)
	{
		var il = new ILCursor(context);
		bool alphaMatch = false;

		while (il.TryGotoNext(
			MoveType.Before,
			i => (i.MatchLdsfld(typeof(GoreID.Sets), nameof(GoreID.Sets.DisappearSpeed)) && !(alphaMatch = false))
			  || (i.MatchLdsfld(typeof(GoreID.Sets), nameof(GoreID.Sets.DisappearSpeedAlpha)) && (alphaMatch = true)),
			i => i.MatchLdarg(0),
			i => i.MatchLdfld(typeof(Gore), nameof(Gore.type)),
			i => i.MatchLdelemI4()
		)) {

			il.RemoveRange(4);
			il.Emit(OpCodes.Ldarg_0);

			if (alphaMatch) {
				il.EmitDelegate(GetDisappearSpeedAlpha);
			} else {
				il.EmitDelegate(GetDisappearSpeed);
			}
		}
	}

	private static int GetDisappearSpeed(Gore gore)
	{
		int speed = GoreID.Sets.DisappearSpeed[gore.type];

		return speed == 1 && EnableGoreStay ? 0 : speed;
	}

	private static int GetDisappearSpeedAlpha(Gore gore)
	{
		int speed = GoreID.Sets.DisappearSpeedAlpha[gore.type];

		return speed == 1 && EnableGoreStay ? 0 : speed;
	}
}
