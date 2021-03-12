using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Exceptions;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCDamageAudio : GlobalNPC
	{
		public static readonly ModSoundStyle GoreSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSplatter", 2, volume: 0.475f, pitchVariance: 0.25f);
		public static readonly ModSoundStyle FleshHitSound = new ModSoundStyle($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/FleshHit", 4, volume: 0.5f, pitchVariance: 0.25f);

		public override void Load()
		{
			//Hook for making the PlayHitSound method control whether or not to play the original hitsound.
			IL.Terraria.NPC.StrikeNPC += context => {
				var cursor = new ILCursor(context);

				//Match 'if (HitSound != null)'
				ILLabel onCheckFailureLabel = null;

				if(!cursor.TryGotoNext(
					MoveType.After,
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(NPC), nameof(NPC.HitSound)),
					i => i.Match(OpCodes.Ldnull),
					i => i.Match(OpCodes.Cgt_Un),
					i => i.Match(OpCodes.Stloc_S),
					i => i.Match(OpCodes.Ldloc_S),
					i => i.MatchBrfalse(out onCheckFailureLabel)
				)) {
					throw new ILMatchException(context, initiator: this);
				}

				cursor.Emit(OpCodes.Ldarg_0);
				cursor.EmitDelegate<Func<NPC, bool>>(npc => !npc.TryGetGlobalNPC(out NPCDamageAudio npcDamageAudio) || npcDamageAudio.PlayHitSound(npc));
				cursor.Emit(OpCodes.Brfalse, onCheckFailureLabel);
			};

			//Hook for making the PlayDeathSound method control whether or not to play the original death sound.
			IL.Terraria.NPC.checkDead += context => {
				var cursor = new ILCursor(context);

				//Match 'if (DeathSound != null)'
				ILLabel onCheckFailureLabel = null;

				if(!cursor.TryGotoNext(
					MoveType.After,
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(NPC), nameof(NPC.DeathSound)),
					i => i.Match(OpCodes.Ldnull),
					i => i.Match(OpCodes.Cgt_Un),
					i => i.Match(OpCodes.Stloc_S),
					i => i.Match(OpCodes.Ldloc_S),
					i => i.MatchBrfalse(out onCheckFailureLabel)
				)) {
					throw new ILMatchException(context, initiator: this);
				}

				cursor.Emit(OpCodes.Ldarg_0);
				cursor.EmitDelegate<Func<NPC, bool>>(npc => !npc.TryGetGlobalNPC(out NPCDamageAudio npcDamageAudio) || npcDamageAudio.PlayDeathSound(npc));
				cursor.Emit(OpCodes.Brfalse, onCheckFailureLabel);
			};
		}

		public bool PlayHitSound(NPC npc)
		{
			if(!npc.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore)) {
				return true;
			}

			if(npc.HitSound == SoundID.NPCHit1 && npcBloodAndGore.LastHitBloodAmount > 0) {
				SoundEngine.PlaySound(FleshHitSound, npc.Center);

				return false;
			}

			return true;
		}
		public bool PlayDeathSound(NPC npc)
		{
			if(!npc.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore)) {
				return true;
			}

			if((npc.DeathSound == SoundID.NPCDeath1 || npc.DeathSound == SoundID.NPCDeath2) && npcBloodAndGore.LastHitBloodAmount > 0) {
				SoundEngine.PlaySound(GoreSound, npc.Center);

				return false;
			}

			return true;
		}
	}
}
