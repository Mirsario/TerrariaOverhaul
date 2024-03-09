using System;
using System.IO;
using System.Linq;
using System.Text;
using Terraria.Localization;
using Terraria.ModLoader;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Syntax;
using IOResult = TerrariaOverhaul.Core.Configuration.ConfigIO.Result;

namespace TerrariaOverhaul.Core.Configuration;

public static class TomlConfig
{
	public static readonly ConfigFormat Format;

	static TomlConfig()
	{
		Format.ReadConfig = ReadConfig;
		Format.WriteConfig = WriteConfig;
		Format.Extension = ".toml";
	}

	public static IOResult WriteConfig(Stream stream, in ConfigExport configExport)
	{
		var mod = OverhaulMod.Instance;

		var rootTable = new TableSyntax();
		var documentSyntax = new DocumentSyntax {
			Tables = { rootTable },
		};

		if (configExport.ModVersion is Version modVersion) {
			rootTable.Items.Add("ModVersion", modVersion.ToString());
		}

		var categoriesByName = ConfigSystem.CategoriesByName;
		var valuesByEntry = configExport.ValuesByEntry;

		foreach (var categoryPair in categoriesByName.OrderBy(p => p.Key)) {
			var categoryTable = new TableSyntax(categoryPair.Key);

			categoryTable.AddLeadingTriviaNewLine();

			foreach (var entry in categoryPair.Value.EntriesByName.OrderBy(p => p.Key).Select(p => p.Value)) {
				if (!valuesByEntry.TryGetValue(entry.Name, out object? value)) {
					continue;
				}

				ValueSyntax valueSyntax = ObjectToSyntax(value);
				var keyValueSyntax = new KeyValueSyntax(entry.Name, valueSyntax);

				string descriptionKey = $"Mods.{mod.Name}.Configuration.{entry.Category}.{entry.Name}.Description";
				string description = Language.GetTextValue(descriptionKey);

				keyValueSyntax.AddLeadingTriviaNewLine();
				if (description != descriptionKey) {
					foreach (string line in description.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.TrimEntries)) {
						keyValueSyntax.AddLeadingTrivia(new SyntaxTrivia(TokenKind.Comment, $"\t# {line}\r\n"));
					}
				}

				keyValueSyntax.AddLeadingTrivia(new SyntaxTrivia(TokenKind.Whitespaces, $"\t"));
				categoryTable.Items.Add(keyValueSyntax);
			}

			documentSyntax.Tables.Add(categoryTable);
		}

		using var streamWriter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
		documentSyntax.WriteTo(streamWriter);

		return IOResult.Success;
	}

	public static IOResult ReadConfig(Stream stream, out ConfigExport configExport)
	{
		using var streamReader = new StreamReader(stream);
		string text = streamReader.ReadToEnd();

		try {
			var rootTable = Toml.ToModel(text);

			if (rootTable == null) {
				configExport = default;
				return IOResult.FileBroken;
			}

			configExport.ModVersion = null;

			if (rootTable.TryGetValue("ModVersion", out object? modVersionToken) && modVersionToken is string modVersionString) {
				if (Version.TryParse(modVersionString, out var version)) {
					configExport.ModVersion = version;
				}
			}

			bool hadErrors = false;
			var categoriesByName = ConfigSystem.CategoriesByName;

			configExport.ValuesByEntry = new();

			foreach (var categoryPair in rootTable) {
				if (categoryPair.Value is not TomlTable categoryTable) {
					continue;
				}

				if (!categoriesByName.TryGetValue(categoryPair.Key, out var category)) {
					continue;
				}

				foreach (var entryPair in categoryTable) {
					if (!category.EntriesByName.TryGetValue(entryPair.Key, out var entry)) {
						continue;
					}

					object? value = null;
					using var _ = new Logging.QuietExceptionHandle();

					try {
						value = Convert.ChangeType(entryPair.Value, entry.ValueType);
					}
					catch { }

					if (value != null) {
						configExport.ValuesByEntry[entry.Name] = value;
					} else {
						hadErrors = true;
					}
				}
			}

			return hadErrors ? IOResult.HadErrors : IOResult.Success;
		}
		catch {
			configExport = default;
			return IOResult.FileBroken;
		}
	}

	// If Tomlyn already has something for this - let me know.
	private static ValueSyntax ObjectToSyntax(object value)
	{
		return value switch {
			bool
				=> new BooleanValueSyntax(Convert.ToBoolean(value)),
			byte or sbyte or ushort or short or uint or int or ulong or long
				=> new IntegerValueSyntax(Convert.ToInt64(value)),
			float or double
				=> new FloatValueSyntax(Convert.ToDouble(value)),
			string
				=> new StringValueSyntax(Convert.ToString(value)!),
			_ => throw new NotImplementedException(),
		};
	}
}
