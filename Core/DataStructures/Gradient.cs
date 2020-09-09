using System;
using Microsoft.Xna.Framework;

namespace TerrariaOverhaul.Core.DataStructures
{
	public class Gradient<T>
	{
		public class GradientKey
		{
			public float time;
			public T value;

			public GradientKey(float time,T value)
			{
				this.time=  time;
				this.value= value;
			}
		}

		public static Func<T,T,float,T> LerpFunc { protected get; set; }

		public GradientKey[] keys;

		static Gradient()
		{
			Gradient<float>.LerpFunc = MathHelper.Lerp;
			Gradient<double>.LerpFunc = (a,b,time) => a+(b-a)*(time<0d ? 0f : time>1d ? 1d : time);

			Gradient<int>.LerpFunc = (left,right,t) => (int)Math.Round(MathHelper.Lerp(left,right,t));
			Gradient<uint>.LerpFunc = (left,right,t) => (uint)Math.Round(MathHelper.Lerp(left,right,t));
			Gradient<long>.LerpFunc = (left,right,t) => (long)Math.Round(MathHelper.Lerp(left,right,t));
			Gradient<ulong>.LerpFunc = (left,right,t) => (ulong)Math.Round(MathHelper.Lerp(left,right,t));

			Gradient<Color>.LerpFunc = Color.Lerp;

			Gradient<Vector2>.LerpFunc = Vector2.Lerp;
			Gradient<Vector3>.LerpFunc = Vector3.Lerp;
			Gradient<Vector4>.LerpFunc = Vector4.Lerp;
		}

		public Gradient(float[] positions,T[] values)
		{
			if(LerpFunc==null) {
				throw new NotSupportedException($"Gradient<{typeof(T).Name}>.{nameof(Gradient<float>.LerpFunc)} is not defined.");
			}

			if(positions.Length!=values.Length || positions.Length==0) {
				throw new ArgumentException("Array lengths must be equal and not be zero.");
			}

			keys = new GradientKey[positions.Length];

			for(int i = 0;i<keys.Length;i++) {
				keys[i] = new GradientKey(positions[i],values[i]);
			}
		}

		public T GetValue(float time)
		{
			GradientKey left = null;
			GradientKey right = null;

			for(int i = 0;i<keys.Length;i++) {
				if(left==null || keys[i].time>left.time && keys[i].time<=time) {
					left = keys[i];
				}
			}

			for(int i = keys.Length-1;i>=0;i--) {
				if(right==null || keys[i].time<right.time && keys[i].time>=time) {
					right = keys[i];
				}
			}

			return left.time==right.time ? left.value : LerpFunc(left.value,right.value,(time-left.time)/(right.time-left.time));
		}
	}
}