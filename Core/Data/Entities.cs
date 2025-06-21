namespace TerrariaOverhaul.Core.Data;

public static class Entities
{
	public static DataEntity Create()
		=> DataStorage.CreateEntity();

	public static Query Query()
		=> DataStorage.CreateQuery().Without<PrefabInfo>();
}
