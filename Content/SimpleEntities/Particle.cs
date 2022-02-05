using Microsoft.Xna.Framework;
using Terraria;
using TerrariaOverhaul.Common.Systems.Camera;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.Systems.SimpleEntities;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Content.SimpleEntities
{
	public abstract class Particle : SimpleEntity
	{
		public const float MaxParticleDistance = 3000f;
		public const float MaxParticleDistanceSqr = MaxParticleDistance * MaxParticleDistance;

		public float alpha = 1f;
		public float rotation;
		public Vector2 position;
		public Vector2 velocity;
		public Vector2 velocityScale = Vector2.One;
		public Vector2 scale = Vector2.One;
		public Vector2 gravity = new(0f, 10f);
		public Color color = Color.White;

		public int LifeTime { get; private set; }

		public virtual bool CollidesWithTiles => true;

		public override void Update()
		{
			if (Vector2.DistanceSquared(position, CameraSystem.ScreenCenter) >= MaxParticleDistanceSqr || position.HasNaNs()) {
				Destroy();

				return;
			}

			velocity += gravity * TimeSystem.LogicDeltaTime;

			if (CollidesWithTiles && Main.tile.TryGet((int)(position.X / 16), (int)(position.Y / 16), out var tile)) {
				if (tile.HasTile && Main.tileSolid[tile.TileType]) {
					OnTileContact(tile, out bool destroy);

					if (destroy) {
						Destroy();

						return;
					}
				} else if (tile.LiquidAmount > 0) {
					OnLiquidContact(tile, out bool destroy);

					if (destroy) {
						Destroy(true);
						return;
					}
				}
			}

			position += velocity * velocityScale * TimeSystem.LogicDeltaTime;
			LifeTime++;
		}

		protected virtual void OnLiquidContact(Tile tile, out bool destroy)
		{
			destroy = false;
		}

		protected virtual void OnTileContact(Tile tile, out bool destroy)
		{
			destroy = true;
		}
	}
}
