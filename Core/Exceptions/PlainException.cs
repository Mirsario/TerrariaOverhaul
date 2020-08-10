using System;

namespace TerrariaOverhaul.Core.Exceptions
{
	public class PlainException : Exception
	{
		public PlainException(string message) : base(message) { }
	}
}
