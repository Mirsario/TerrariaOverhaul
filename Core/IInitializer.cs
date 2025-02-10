using Terraria.ModLoader;

namespace TerrariaOverhaul.Core;

// Just makes the Unload()'s definition optional.
internal interface IInitializer : ILoadable
{
	void ILoadable.Unload() { }
}
