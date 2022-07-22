using System.Text.RegularExpressions;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Localization
{
	// The vanilla classes are too all over the place.
	public sealed class Text
	{
		private enum Type
		{
			Literal,
			Localized,
		}

		private static readonly Regex modKeyRegex = new(@"Mods\.(\w+)\.([\s\S]+)", RegexOptions.Compiled);

		private static int ActiveCultureId => Language.ActiveCulture.LegacyId;

		private readonly Type type;
		private readonly string source;
		
		private string? cachedValue;
		private int lastActiveCultureId;
		private int lastLanguageRefreshCount;

		public string Value
			=> NeedsRecalculation ? CalculateValue() : cachedValue!;

		private bool NeedsRecalculation {
			get {
				// Always calculate if haven't before.
				if (cachedValue == null) {
					return true;
				}

				// Refresh whenever 'LocalizationLoader.RefreshModLanguage(...)' is called.
				if (TextSystem.LanguageRefreshCount != lastLanguageRefreshCount) {
					return true;
				}

				// Refresh on language changes.
				if (ActiveCultureId != lastActiveCultureId) {
					return true;
				}

				return false;
			}
		}

		private Text(string source, Type type)
		{
			this.source = source;
			this.type = type;

			UpdateDeltas();
		}

		public override string ToString()
			=> Value;

		private string CalculateValue()
		{
			UpdateDeltas();

			switch (type) {
				case Type.Localized:
					cachedValue = Language.GetTextValue(source);

					if (cachedValue == source && lastLanguageRefreshCount == 0) {
						var modTranslation = LocalizationLoader.GetOrCreateTranslation(source, defaultEmpty: true);
						
						cachedValue = modTranslation.GetTranslation(ActiveCultureId);

						if (string.IsNullOrWhiteSpace(cachedValue) || cachedValue == source) {
							cachedValue = "Localizations loading...";
						}
					}
					
					break;
				default:
					cachedValue = source;
					break;
			}

			return cachedValue;
		}

		private void UpdateDeltas()
		{
			lastActiveCultureId = ActiveCultureId;
			lastLanguageRefreshCount = TextSystem.LanguageRefreshCount;
		}

		public static Text Literal(string literal)
			=> new(literal, Type.Literal);

		public static Text Localized(string key)
			=> new(key, Type.Localized);

		public static implicit operator string(Text text) => text.Value;
	}
}
