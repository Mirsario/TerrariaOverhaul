using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Content.SimpleEntities
{
	[Autoload(Side = ModSide.Client)]
	public class BloodParticle : Particle
	{
		public static readonly SoundStyle BloodDripSound = new ModSoundStyle(nameof(TerrariaOverhaul), "Assets/Sounds/Gore/BloodDrip", 14, volume: 0.3f, pitchVariance: 0.2f);

		private static Asset<Texture2D> texture;
		private static List<List<Color>> bloodColorRecordingLists;

		private Rectangle frame;

		//Load-time
		public override void Load(Mod mod)
		{
			texture = ModContent.GetTexture(ModUtils.GetTypePath(GetType()));
			bloodColorRecordingLists = new List<List<Color>>();
		}
		public override void Unload()
		{
			texture = null;

			if(bloodColorRecordingLists != null) {
				bloodColorRecordingLists.Clear();

				bloodColorRecordingLists = null;
			}
		}
		//In-game
		public override void Init()
		{
			frame = new Rectangle(0, 8 * Main.rand.Next(3), 8, 8);
			gravity = new Vector2(0f, 300f);
			velocityScale = Vector2.One * Main.rand.NextFloat(0.5f, 1f);

			for(int i = 0; i < bloodColorRecordingLists.Count; i++) {
				bloodColorRecordingLists[i].Add(color);
			}
		}
		public override void Draw(SpriteBatch sb)
		{
			var source = frame;
			var dest = new Rectangle(
				(int)(position.X - Main.screenPosition.X - source.Width * 0.5f * (scale.X - 1f)),
				(int)(position.Y - Main.screenPosition.Y - source.Height * 0.5f * (scale.Y - 1f)),
				(int)(source.Width * scale.X),
				(int)(source.Height * scale.Y)
			);

			var origin = source.Size() * 0.5f;
			var usedColor = Lighting.GetColor((int)(position.X / 16f), (int)(position.Y / 16f), color.WithAlpha((byte)(color.A * alpha)));

			sb.Draw(texture.Value, dest, source, usedColor, rotation, origin, SpriteEffects.None, 0f);
		}

		protected override void OnTileContact(Tile tile, out bool destroy)
		{
			destroy = true;

			if(Main.rand.Next(50) == 0) {
				SoundEngine.PlaySound(BloodDripSound, position);
			}
		}

		/// <summary> Returns a list of colors of blood that has been created during execution of the provided delegate. </summary>
		public static List<Color> RecordBloodColors(Action innerAction)
		{
			var list = new List<Color>();

			bloodColorRecordingLists.Add(list);

			try {
				innerAction();
			}
			finally {
				bloodColorRecordingLists.Remove(list);
			}

			return list;
		}
	}
}
