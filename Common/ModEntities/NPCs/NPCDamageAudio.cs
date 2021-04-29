using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Hooks.Items;
using TerrariaOverhaul.Common.Systems.DamageSources;
using TerrariaOverhaul.Core.Exceptions;

namespace TerrariaOverhaul.Common.ModEntities.NPCs
{
	public class NPCDamageAudio : GlobalNPC
	{
		public static readonly ModSoundStyle GoreSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/GoreSplatter", 2, volume: 0.475f, pitchVariance: 0.25f);
		public static readonly ModSoundStyle FleshHitSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/HitEffects/FleshHit", 4, volume: 0.5f, pitchVariance: 0.25f);

		public override void Load()
		{
			//Hook for making the PlayHitSound method control whether or not to play the original hitsound.
			IL.Terraria.NPC.StrikeNPC += context => {
				var cursor = new ILCursor(context);

				//Match 'if (HitSound != null)'
				ILLabel onCheckFailureLabel = null;

				cursor.GotoNext(
					MoveType.After,
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(NPC), nameof(NPC.HitSound))
				);
				cursor.GotoNext(
					MoveType.After,
					i => i.MatchBrfalse(out onCheckFailureLabel)
				);

				cursor.Emit(OpCodes.Ldarg_0);
				cursor.EmitDelegate<Func<NPC, bool>>(npc => !npc.TryGetGlobalNPC(out NPCDamageAudio npcDamageAudio) || npcDamageAudio.PlayHitSound(npc));
				cursor.Emit(OpCodes.Brfalse, onCheckFailureLabel);
			};

			//Hook for making the PlayDeathSound method control whether or not to play the original death sound.
			IL.Terraria.NPC.checkDead += context => {
				var cursor = new ILCursor(context);

				//Match 'if (DeathSound != null)'
				ILLabel onCheckFailureLabel = null;

				cursor.GotoNext(
					MoveType.After,
					i => i.Match(OpCodes.Ldarg_0),
					i => i.MatchLdfld(typeof(NPC), nameof(NPC.DeathSound))
				);
				cursor.GotoNext(
					MoveType.After,
					i => i.MatchBrfalse(out onCheckFailureLabel)
				);

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

			bool playOriginalSound = true;
			SoundStyle customSoundStyle = null;

			if(npcBloodAndGore.LastHitBloodAmount > 0) {
				customSoundStyle = FleshHitSound;
				playOriginalSound = npc.HitSound != SoundID.NPCHit1;
			}

			var damageSource = DamageSourceSystem.CurrentDamageSource;

			//Call item hit sound modification hooks.
			if(damageSource != null && damageSource.Source is Item item && damageSource.Parent?.Source is Player player) {
				IModifyItemNPCHitSound.Hook.Invoke(item, player, npc, ref customSoundStyle, ref playOriginalSound);
			}

			if(customSoundStyle != null) {
				SoundEngine.PlaySound(customSoundStyle, npc.Center);
			}

			return playOriginalSound;
		}
		public bool PlayDeathSound(NPC npc)
		{
			if(!npc.TryGetGlobalNPC(out NPCBloodAndGore npcBloodAndGore)) {
				return true;
			}

			bool playOriginalSound = true;
			SoundStyle customSoundStyle = null;

			if(npcBloodAndGore.LastHitBloodAmount > 0) {
				customSoundStyle = GoreSound;
				playOriginalSound = npc.DeathSound != SoundID.NPCDeath1;
			}

			var damageSource = DamageSourceSystem.CurrentDamageSource;

			//Call item death sound modification hooks.
			if(damageSource != null && damageSource.Source is Item item && damageSource.Parent?.Source is Player player) {
				IModifyItemNPCDeathSound.Hook.Invoke(item, player, npc, ref customSoundStyle, ref playOriginalSound);
			}

			if(customSoundStyle != null) {
				SoundEngine.PlaySound(customSoundStyle, npc.Center);
			}

			return playOriginalSound;
		}
	}
}
