using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.BloodAndGore;
using TerrariaOverhaul.Common.DamageSources;
using TerrariaOverhaul.Common.Hooks.Items;

namespace TerrariaOverhaul.Common.Damage;

public class NPCDamageAudio : GlobalNPC
{
	public static readonly SoundStyle GoreSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSplatter", 2) {
		Volume = 0.475f,
		PitchVariance = 0.25f,
	};
	public static readonly SoundStyle FleshHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/FleshHit", 4) {
		Volume = 0.5f,
		PitchVariance = 0.25f,
	};

	public override void Load()
	{
		// Hook for making the PlayHitSound method control whether or not to play the original hitsound.
		IL_NPC.StrikeNPC_HitInfo_bool_bool += context => {
			var cursor = new ILCursor(context);

			// Match 'if (HitSound != null)'
			ILLabel? onCheckFailureLabel = null;

			cursor.GotoNext(
				MoveType.After,
				i => i.Match(OpCodes.Ldarg_0),
				i => i.MatchLdflda(typeof(NPC), nameof(NPC.HitSound))
			);
			cursor.GotoNext(
				MoveType.After,
				i => i.MatchBrfalse(out onCheckFailureLabel)
			);

			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate<Func<NPC, bool>>(npc => !npc.TryGetGlobalNPC(out NPCDamageAudio npcDamageAudio) || PlayHitSound(npc));
			cursor.Emit(OpCodes.Brfalse, onCheckFailureLabel!);
		};

		// Hook for making the PlayDeathSound method control whether or not to play the original death sound.
		IL_NPC.checkDead += context => {
			var cursor = new ILCursor(context);

			// Match
			// 'SoundStyle? style = DeathSound;'
			// to get the style local's id.

			int styleLocalId = -1;

			cursor.GotoNext(
				MoveType.After,
				i => i.Match(OpCodes.Ldarg_0),
				i => i.MatchLdfld(typeof(NPC), nameof(NPC.DeathSound)),
				i => i.MatchStloc(out styleLocalId)
			);

			// Match
			// 'SoundStyle? style = DeathSound;'
			// to get the label and relocate.

			ILLabel? onCheckFailureLabel = null;

			cursor.GotoNext(
				MoveType.After,
				i => i.MatchLdloca(styleLocalId),
				i => i.MatchCall(out _) // Couldn't bother getting this to work - i.MatchCall(typeof(SoundStyle?), "get_HasValue")
			);
			// (Skip Debug NoOPs)
			cursor.GotoNext(
				MoveType.After,
				i => i.MatchBrfalse(out onCheckFailureLabel)
			);

			// Emit extra check that would run our code and then short-circuit on success.

			cursor.Emit(OpCodes.Ldarg_0);
			cursor.EmitDelegate<Func<NPC, bool>>(npc => !npc.TryGetGlobalNPC<NPCDamageAudio>(out _) || PlayDeathSound(npc));
			cursor.Emit(OpCodes.Brfalse, onCheckFailureLabel!);
		};
	}

	private static bool PlayHitSound(NPC npc)
	{
		if (!npc.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore)) {
			return true;
		}

		bool playOriginalSound = true;
		SoundStyle? customSoundStyle = null;

		if (npcBloodAndGore.LastHitBloodAmount > 0) {
			customSoundStyle = FleshHitSound;
			playOriginalSound = npc.HitSound != SoundID.NPCHit1;
		}

		var damageSource = DamageSourceSystem.CurrentDamageSource;

		// Call item hit sound modification hooks.
		if (damageSource != null && damageSource.Source is Item item && damageSource.Parent?.Source is Player player) {
			IModifyItemNPCHitSound.Invoke(item, player, npc, ref customSoundStyle, ref playOriginalSound);
		}

		if (customSoundStyle.HasValue) {
			SoundEngine.PlaySound(customSoundStyle.Value, npc.Center);
		}

		return playOriginalSound;
	}

	private static bool PlayDeathSound(NPC npc)
	{
		if (!npc.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore)) {
			return true;
		}

		bool playOriginalSound = true;
		SoundStyle? customSoundStyle = null;

		if (npcBloodAndGore.LastHitBloodAmount > 0) {
			customSoundStyle = GoreSound;
			playOriginalSound = npc.DeathSound != SoundID.NPCDeath1;
		}

		var damageSource = DamageSourceSystem.CurrentDamageSource;

		// Call item death sound modification hooks.
		if (damageSource != null && damageSource.Source is Item item && damageSource.Parent?.Source is Player player) {
			IModifyItemNPCDeathSound.Invoke(item, player, npc, ref customSoundStyle, ref playOriginalSound);
		}

		if (customSoundStyle.HasValue) {
			SoundEngine.PlaySound(customSoundStyle.Value, npc.Center);
		}

		return playOriginalSound;
	}
}
