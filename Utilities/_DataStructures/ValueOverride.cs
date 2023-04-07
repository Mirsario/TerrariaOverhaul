using System;
using System.Reflection;

#pragma warning disable IDE0064 // Make readonly fields writable

namespace TerrariaOverhaul.Utilities;

//TODO: Unscrew with ref fields when TML moves to .NET 7
public ref struct ValueOverride<T>
{
	private readonly T oldValue;
	private readonly FieldInfo field;
	private readonly object? obj;

	public ValueOverride(Type type, string fieldName, T value)
		: this(type.GetField(fieldName, ReflectionUtils.AnyBindingFlags)!, null, value) { }

	public ValueOverride(Type type, string fieldName, object? obj, T value)
		: this(type.GetField(fieldName)!, obj, value) { }

	public ValueOverride(FieldInfo field, object? obj, T value)
	{
		this.field = field;
		this.obj = obj;
		oldValue = (T)field.GetValue(obj)!;

		field.SetValue(obj, value);
	}

	public void Dispose()
	{
		field?.SetValue(obj, oldValue);

		this = default;
	}
}
