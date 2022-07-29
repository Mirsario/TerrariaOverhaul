using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Utilities;

public abstract class Gradient
{
	static Gradient()
	{
		Gradient<float>.LerpFunc = MathHelper.Lerp;
		Gradient<double>.LerpFunc = (a, b, time) => a + (b - a) * (time < 0d ? 0f : time > 1d ? 1d : time);

		Gradient<int>.LerpFunc = (left, right, t) => (int)Math.Round(MathHelper.Lerp(left, right, t));
		Gradient<uint>.LerpFunc = (left, right, t) => (uint)Math.Round(MathHelper.Lerp(left, right, t));
		Gradient<long>.LerpFunc = (left, right, t) => (long)Math.Round(MathHelper.Lerp(left, right, t));
		Gradient<ulong>.LerpFunc = (left, right, t) => (ulong)Math.Round(MathHelper.Lerp(left, right, t));

		Gradient<Color>.LerpFunc = Color.Lerp;

		Gradient<Vector2>.LerpFunc = Vector2.Lerp;
		Gradient<Vector3>.LerpFunc = Vector3.Lerp;
		Gradient<Vector4>.LerpFunc = Vector4.Lerp;
	}
}

public class Gradient<T> : Gradient
{
	public struct GradientKey
	{
		public float Time;
		public T Value;

		public GradientKey(float time, T value)
		{
			Time = time;
			Value = value;
		}
	}

	public static Func<T, T, float, T>? LerpFunc { protected get; set; }

	private GradientKey[] keys;

	public GradientKey[] Keys {
		get => keys;
		set => keys = value ?? throw new ArgumentNullException(nameof(value));
	}

	public Gradient(params (float position, T value)[] values)
	{
		if (LerpFunc == null) {
			throw new NotSupportedException($"Gradient<{typeof(T).Name}>.{nameof(Gradient<float>.LerpFunc)} is not defined.");
		}

		if (values.Length == 0) {
			throw new ArgumentException("Array length must not be zero.");
		}

		keys = new GradientKey[values.Length];

		for (int i = 0; i < keys.Length; i++) {
			var (position, value) = values[i];

			keys[i] = new GradientKey(position, value);
		}
	}

	public T GetValue(float time)
	{
		if (keys.Length == 0) {
			throw new InvalidOperationException("Gradient length must not be zero.");
		}

		bool leftDefined = false;
		bool rightDefined = false;
		GradientKey left = default;
		GradientKey right = default;

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

		return left.Time == right.Time ? left.Value : LerpFunc!(left.Value, right.Value, (time - left.Time) / (right.Time - left.Time));
	}
}
