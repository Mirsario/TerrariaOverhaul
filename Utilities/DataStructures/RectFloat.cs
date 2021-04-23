using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities.DataStructures
{
	public struct RectFloat
	{
		public static readonly RectFloat Default = new(0f, 0f, 1f, 1f);
		public static readonly RectFloat Empty = new(0f, 0f, 0f, 0f);

		public float x;
		public float y;
		public float width;
		public float height;

		public float Left {
			get => x;
			set => x = value;
		}
		public float Top {
			get => y;
			set => y = value;
		}
		public float Right {
			get => x + width;
			set => x = value - width;
		}
		public float Bottom {
			get => y + height;
			set => y = value - height;
		}
		public Vector2 TopLeft {
			get => new(Left, Top);
			set {
				Left = value.X;
				Top = value.Y;
			}
		}
		public Vector2 TopRight {
			get => new(Right, Top);
			set {
				Right = value.X;
				Top = value.Y;
			}
		}
		public Vector2 BottomLeft {
			get => new(Left, Bottom);
			set {
				Left = value.X;
				Bottom = value.Y;
			}
		}
		public Vector2 BottomRight {
			get => new(Right, Bottom);
			set {
				Right = value.X;
				Bottom = value.Y;
			}
		}
		public Vector2 Position {
			get => TopLeft;
			set => TopLeft = value;
		}
		public Vector2 Size {
			get => new(width, height);
			set {
				width = value.X;
				height = value.Y;
			}
		}
		public Vector4 Points {
			get => new(x, y, x + width, y + height);
			set {
				x = value.X;
				y = value.Y;
				width = value.Z - x;
				height = value.W - y;
			}
		}

		public RectFloat(float x, float y, float width, float height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public override string ToString() => $"[X:{x} Y:{y} Width:{width} Height:{height}]";

		public bool Contains(Vector2 point, bool inclusive = false)
		{
			if(inclusive) {
				return point.X > x && point.X < x + width && point.Y > y && point.Y < y + height;
			}

			return point.X >= x && point.Y <= x + width && point.Y >= y && point.Y <= y + height;
		}

		public static RectFloat FromPoints(Vector4 points) => FromPoints(points.X, points.Y, points.Z, points.W);
		public static RectFloat FromPoints(float x1, float y1, float x2, float y2)
		{
			RectFloat rect;

			rect.x = x1;
			rect.y = y1;
			rect.width = x2 - x1;
			rect.height = y2 - y1;

			return rect;
		}

		public static explicit operator RectFloat(Rectangle rectI) => new(rectI.X, rectI.Y, rectI.Width, rectI.Height);
		public static explicit operator Rectangle(RectFloat rectF) => new((int)rectF.x, (int)rectF.y, (int)rectF.width, (int)rectF.height);
	}
}
