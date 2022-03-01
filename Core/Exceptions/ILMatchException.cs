using System;
using MonoMod.Cil;

namespace TerrariaOverhaul.Core.Exceptions
{
	public sealed class ILMatchException : Exception
	{
		public ILMatchException(ILContext context, string locationInfo = null, object initiator = null) : base(
			$"IL matching error has occured in '{context.Method.Name}'."
			+ (locationInfo != null ? $"\r\nLocation: '{locationInfo}'." : null)
			+ (initiator != null ? $"\r\nInitiator: '{initiator.GetType().FullName}'." : null)
		)
		{ }
	}
}
