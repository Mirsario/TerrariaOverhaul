using Microsoft.Xna.Framework;
using Terraria;

namespace TerrariaOverhaul.Common.Crosshairs
{
	public struct CrosshairImpulse
	{
		public float time;
		public float timeMax;
		public float strength;
		public float rotation;
		public Color? color;
		public bool reversed;

		public CrosshairImpulse(float strength, float timeInSeconds, float rotation = 0f, Color? color = null, bool reversed = false, bool autoRotation = false)
		{
			this.strength = strength;
			this.color = color;
			this.reversed = reversed;

			time = timeMax = timeInSeconds;

			if (autoRotation) {
				if (timeInSeconds < 0.33f) {
					this.rotation = MathHelper.PiOver2;
				} else if (time < 1f) {
					this.rotation = MathHelper.Pi;
				} else {
					this.rotation = MathHelper.TwoPi;
				}

				this.rotation *= -Main.LocalPlayer.direction;
			} else {
				this.rotation = rotation;
			}
		}
	}
}
