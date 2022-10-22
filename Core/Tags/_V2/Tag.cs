namespace TerrariaOverhaul.Core.Tags;

public struct Tag
{
	public readonly int Id;
	public readonly string Name;

	public bool IsValid => Id > 0;

	public Tag(string name)
	{
		Name = name;
		Id = TagSystem.GetTagIdForString(name);
	}

	public override bool Equals(object? obj)
		=> obj is Tag tag && tag.Id == Id;

	public override int GetHashCode()
		=> Id;

	public override string ToString()
		=> $"{Id} - '{Name}'";

	public static bool operator ==(Tag a, Tag b)
		=> a.Id == b.Id;

	public static bool operator !=(Tag a, Tag b)
		=> a.Id != b.Id;

	public static implicit operator Tag(string? name)
		=> name != null ? new(name) : default;
}
