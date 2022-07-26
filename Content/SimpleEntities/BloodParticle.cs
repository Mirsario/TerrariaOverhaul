using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using TerrariaOverhaul.Common.Camera;
using TerrariaOverhaul.Common.Decals;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Content.SimpleEntities;

[Autoload(Side = ModSide.Client)]
public class BloodParticle : Particle
{
	private const int MaxPositions = 3;

	public static readonly SoundStyle BloodDripSound = new($"{nameof(TerrariaOverhaul)}/Assets/Sounds/Gore/BloodDrip", 14) {
		Volume = 0.3f,
		PitchVariance = 0.2f
	};

	private static readonly List<List<Color>> bloodColorRecordingLists = new();

	private Rectangle frame;
	private Vector2[]? positions;

	public override bool CollidesWithTiles => LifeTime >= 3;

	public override void Unload()
	{
		bloodColorRecordingLists?.Clear();
	}

	// In-game
	public override void Init()
	{
		frame = new Rectangle(0, 8 * Main.rand.Next(3), 8, 8);
		gravity = new Vector2(0f, 300f);
		velocityScale = Vector2.One * Main.rand.NextFloat(0.5f, 1f);
		positions = new Vector2[MaxPositions];

		for (int i = 0; i < positions.Length; i++) {
			positions[i] = position;
		}

		for (int i = 0; i < bloodColorRecordingLists.Count; i++) {
			bloodColorRecordingLists[i].Add(color);
		}
	}

	public override void Update()
	{
		if (positions == null) {
			throw new InvalidOperationException("Not initialized.");
		}

		// Track old positions.
		Array.Copy(positions, 0, positions, 1, positions.Length - 1);

		positions[0] = position;

		base.Update();
	}

	public override void Draw(SpriteBatch sb)
	{
		if (Vector2.DistanceSquared(position, CameraSystem.ScreenCenter) > Main.screenWidth * Main.screenWidth * 2 || position.HasNaNs()) {
			Destroy();
			return;
		}

		if (positions == null) {
			throw new InvalidOperationException("Not initialized.");
		}

		var usedColor = Lighting.GetColor((int)(position.X / 16f), (int)(position.Y / 16f), color);

		usedColor.A = (byte)(color.A * alpha);

		if (usedColor != default) {
			var lineStart = position;
			var lineEnd = positions[positions.Length - 1];

			sb.DrawLine(lineStart - Main.screenPosition, lineEnd - Main.screenPosition, usedColor, 2);
		}
	}

	protected override void OnTileContact(Tile tile, out bool destroy)
	{
		destroy = true;

		if (Main.rand.Next(50) == 0) {
			SoundEngine.PlaySound(BloodDripSound, position);
		}

		DecalSystem.AddDecals(position + velocity.SafeNormalize(default) * Main.rand.NextFloat(5f), color);
	}

	protected override void OnDestroyed(bool allowEffects)
	{
		base.OnDestroyed(allowEffects);

		positions = null;
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
