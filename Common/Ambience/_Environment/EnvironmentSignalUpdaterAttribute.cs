using System;

namespace TerrariaOverhaul.Common.Ambience;

[AttributeUsage(AttributeTargets.Method)]
public sealed class EnvironmentSignalUpdaterAttribute : Attribute
{
	public readonly string? TagNameOverride;

	public EnvironmentSignalUpdaterAttribute(string? tagNameOverride = null)
	{
		TagNameOverride = tagNameOverride;
	}
}
