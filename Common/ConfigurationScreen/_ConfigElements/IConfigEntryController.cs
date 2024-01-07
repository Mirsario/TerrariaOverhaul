using TerrariaOverhaul.Core.Configuration;

namespace TerrariaOverhaul.Common.ConfigurationScreen;

public interface IConfigEntryController
{
	object? Value { get; set;  }
}
