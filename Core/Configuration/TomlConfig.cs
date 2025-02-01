// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
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
		var entryValuesByName = configExport.EntryValuesByName;

		foreach (var categoryPair in categoriesByName.OrderBy(p => p.Key)) {
			var categoryTable = new TableSyntax(categoryPair.Key);

			categoryTable.AddLeadingTriviaNewLine();

			foreach (var entry in categoryPair.Value.EntriesByName.OrderBy(p => p.Key).Select(p => p.Value)) {
				if (entry.Category != categoryPair.Key) {
					continue;
				}

				if (!entryValuesByName.TryGetValue(entry.Name, out object? value)) {
					continue;
				}

				ValueSyntax valueSyntax = ObjectToSyntax(value);
				var keyValueSyntax = new KeyValueSyntax(entry.Name, valueSyntax);

				keyValueSyntax.AddLeadingTriviaNewLine();

				string? description = entry.Description?.Value;
				string? descriptionKey = entry.Description?.Key;

				if (!string.IsNullOrWhiteSpace(description) && description != descriptionKey) {
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

			configExport.EntryValuesByName = new();

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
						value = ConvertValue(entryPair.Value, entry.ValueType);
					}
					catch { }

					if (value != null) {
						configExport.EntryValuesByName[entry.Name] = value;
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

	private static object? ConvertValue(object? value, Type valueType)
	{
		if (value is TomlArray tomlArray) {
			if (valueType.IsArray) {
				var array = Array.CreateInstance(valueType.GetElementType()!, tomlArray.Count);
				for (int i = 0; i < tomlArray.Count; i++) { array.SetValue(tomlArray[i], i); }
				return array;
			}
			
			if (typeof(IList).IsAssignableFrom(valueType) && valueType.GetConstructor(Type.EmptyTypes) is { } ctor) {
				var list = (IList)ctor.Invoke([]);
				foreach (object? item in tomlArray) { list.Add(item); }
				return list;
			}
			
			throw new InvalidOperationException($"Unable to handle array type: '{valueType.Name}'.");
		}

		return Convert.ChangeType(value, valueType);
	}

	// If Tomlyn already has something for this - let me know.
	private static ValueSyntax ObjectToSyntax(object value)
	{
		switch (value) {
			case bool:
				return new BooleanValueSyntax(Convert.ToBoolean(value));
			case byte or sbyte or ushort or short or uint or int or ulong or long:
				return new IntegerValueSyntax(Convert.ToInt64(value));
			case float or double:
				return new FloatValueSyntax(Convert.ToDouble(value));
			case string:
				return new StringValueSyntax(Convert.ToString(value)!);
			default: {
				if (value is IEnumerable enumerable) {
					var arraySyntax = new ArraySyntax(Array.Empty<int>()); // () is broken!

					foreach (object item in enumerable)
						arraySyntax.Items.Add(new ArrayItemSyntax { Value = ObjectToSyntax(item) });

					return arraySyntax;
				}

				throw new NotImplementedException();
			}
		};
	}
}
