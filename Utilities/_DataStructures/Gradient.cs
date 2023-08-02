using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

public class Gradient
{
	internal Gradient() { }

	static Gradient()
	{
		Gradient<float>.Lerp = MathHelper.Lerp;
		Gradient<double>.Lerp = (a, b, time) => a + (b - a) * (time < 0d ? 0f : time > 1d ? 1d : time);

		Gradient<int>.Lerp = (left, right, t) => (int)Math.Round(MathHelper.Lerp(left, right, t));
		Gradient<uint>.Lerp = (left, right, t) => (uint)Math.Round(MathHelper.Lerp(left, right, t));
		Gradient<long>.Lerp = (left, right, t) => (long)Math.Round(MathHelper.Lerp(left, right, t));
		Gradient<ulong>.Lerp = (left, right, t) => (ulong)Math.Round(MathHelper.Lerp(left, right, t));

		Gradient<Color>.Lerp = Color.Lerp;

		Gradient<Vector2>.Lerp = Vector2.Lerp;
		Gradient<Vector3>.Lerp = Vector3.Lerp;
		Gradient<Vector4>.Lerp = Vector4.Lerp;
	}
}

public sealed class Gradient<T> : Gradient
{
	public struct Key
	{
		public float Time;
		public T Value;

		public Key(float time, T value)
		{
			Time = time;
			Value = value;
		}

		public static implicit operator Key((float time, T value) tuple)
			=> new(tuple.time, tuple.value);
	}

	public delegate T LerpDelegate(T a, T b, float step);

	public static LerpDelegate? Lerp { private get; set; }

	private Key[] keys = Array.Empty<Key>();

	public ReadOnlySpan<Key> Keys {
		get => keys;
		set {
			if (value.Length <= 0) {
				throw new InvalidOperationException("At least 1 key must be specified.");
			}

			keys = value.ToArray();
		}
	}

	public Gradient(params Key[] values) : this()
	{
		Keys = values;
	}

	public Gradient(ReadOnlySpan<Key> values) : this()
	{
		Keys = values;
	}

	private Gradient()
	{
		if (Lerp == null) {
			throw new NotSupportedException($"Gradient<{typeof(T).Name}>.{nameof(Gradient<float>.Lerp)} is not defined.");
		}
	}

	public T GetValue(float time)
	{
		if (keys.Length == 0) {
			throw new InvalidOperationException("Gradient length must not be zero.");
		}

		bool leftDefined = false;
		bool rightDefined = false;
		Key left = default;
		Key right = default;

		for (int i = 0; i < keys.Length; i++) {
			if (!leftDefined || keys[i].Time > left.Time && keys[i].Time <= time) {
				left = keys[i];
				leftDefined = true;
			}
		}

		for (int i = keys.Length - 1; i >= 0; i--) {
			if (!rightDefined || keys[i].Time < right.Time && keys[i].Time >= time) {
				right = keys[i];
				rightDefined = true;
			}
		}

		if (!leftDefined || !rightDefined) {
			throw new InvalidOperationException("No keys found. This shouldn't happen.");
		}

		return left.Time == right.Time ? left.Value : Lerp!(left.Value, right.Value, (time - left.Time) / (right.Time - left.Time));
	}
}
