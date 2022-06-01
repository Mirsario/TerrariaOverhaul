using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Initializers;
using Terraria.IO;

namespace TerrariaOverhaul.Common.MainMenuOverlays
{
	public class MusicPackMenuButton : MenuLink
	{
		private enum MusicPackState
		{
			Missing,
			Disabled,
			Enabled
		}

		private static DateTime lastRefresh;
		private static MusicPackState? cachedState;

		public string TextWhenMissing { get; }
		public string TextWhenDisabled { get; }

		public override bool IsActive => GetState() != MusicPackState.Enabled;

		public MusicPackMenuButton(string textWhenMissing, string textWhenDisabled, string url) : base(textWhenMissing, url)
		{
			TextWhenMissing = textWhenMissing;
			TextWhenDisabled = textWhenDisabled;
		}

		public override void Update(Vector2 position)
		{
			string oldText = Text;

			if (GetState() == MusicPackState.Missing) {
				Text = TextWhenMissing;
			} else {
				Text = TextWhenDisabled;
			}

			if (Text != oldText) {
				Size = Font.Value.MeasureString(Text) * Scale;
			}

			base.Update(position);
		}

		protected override void OnClicked()
		{
			switch (GetState()) {
				case MusicPackState.Missing:
					base.OnClicked(); // Url open
					break;
				case MusicPackState.Disabled:
					var createdResourcePackList = AssetInitializer.CreateResourcePackList(Main.instance.Services);
					var musicPack = createdResourcePackList.AllPacks.FirstOrDefault(IsMusicPack);
					
					if (musicPack != null) {
						musicPack.IsEnabled = true;
						musicPack.SortingOrder = -1000;

						Main.AssetSourceController.UseResourcePacks(new ResourcePackList(Main.AssetSourceController.ActiveResourcePackList.EnabledPacks.Prepend(musicPack)));
						Main.SaveSettings();
					}

					cachedState = null;
					break;
			}
		}

		private static MusicPackState GetState()
		{
			var now = DateTime.Now;

			if (cachedState == null || (now - lastRefresh).TotalSeconds > 5d) {
				var activeResourcePackList = Main.AssetSourceController.ActiveResourcePackList;
				var createdResourcePackList = AssetInitializer.CreateResourcePackList(Main.instance.Services);

				if (activeResourcePackList.EnabledPacks.Any(IsMusicPack)) {
					cachedState = MusicPackState.Enabled;
				} else if (createdResourcePackList.AllPacks.Any(IsMusicPack)) {
					cachedState = MusicPackState.Disabled;
				} else {
					cachedState = MusicPackState.Missing;
				}

				lastRefresh = now;
			}

			return cachedState.Value;
		}

		private static bool IsMusicPack(ResourcePack pack)
			=> pack.Name == "Terraria Overhaul Music Pack";
	}
}
