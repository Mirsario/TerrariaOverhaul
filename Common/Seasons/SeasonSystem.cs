using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Common.Seasons;

public partial class SeasonSystem : ModSystem
{
	private static Season[]? seasons;
	private static int currentSeasonId;

	public static int SeasonLength { get; set; } = 15;
	public static int CurrentDay { get; private set; }

	public static ReadOnlySpan<Season> Seasons => seasons != null ? seasons : throw new InvalidOperationException("Trying to get Seasons' data before they are initialized.");
	public static int SeasonCount => Seasons.Length;
	public static int CurrentSeasonDay => CurrentDay % SeasonLength;
	public static Season CurrentSeason => Seasons[currentSeasonId];
	public static Season NextSeason => Seasons[(currentSeasonId + 1) % SeasonCount];

	public static event Action<Season>? OnSeasonActivated;

	public override void PostSetupContent()
	{
		seasons = new Season[] {
			ModContent.GetInstance<Spring>(),
			ModContent.GetInstance<Summer>(),
			ModContent.GetInstance<Autumn>(),
			ModContent.GetInstance<Winter>()
		};

		for (int i = 0; i < seasons.Length; i++) {
			var season = seasons[i];

			season.Id = i;

			season.Init();
		}
	}

	public override void Unload()
	{
		if (seasons != null) {
			foreach (Season season in seasons) {
				season?.Components.Dispose();
			}

			seasons = null;
		}
	}

	public override void PreUpdateWorld()
	{
		if (seasons == null) {
			return;
		}

		foreach (var season in seasons) {
			foreach (var component in season.Components) {
				component.OnUpdate(season);
			}
		}
	}

	public static bool Is<T>() where T : Season
		=> CurrentSeason is T;

	public static void AdvanceSeasonTime()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			return;
		}

		SetDate(CurrentDay + 1, true);
	}

	public static void SetDate(int dayNum, bool arrival)
	{
		int seasonId = currentSeasonId;

		CurrentDay = dayNum;

		int newSeasonId = CurrentDay / SeasonLength;

		if (seasonId != newSeasonId) {
			SetSeason(newSeasonId, arrival);
		}
	}

	public static void SetSeason<T>(bool arrival) where T : Season
		=> SetSeason(ModContent.GetInstance<T>(), arrival);

	public static void SetSeason(Season season, bool arrival)
		=> SetSeason(season.Id, arrival);

	public static void SetSeason(int seasonId, bool arrival)
	{
		if (seasonId != currentSeasonId) {
			var oldSeason = CurrentSeason;

			currentSeasonId = seasonId;

			var newSeason = CurrentSeason;

			if (newSeason != oldSeason) {
				OnSeasonActivated?.Invoke(newSeason);

				foreach (var component in oldSeason.Components) {
					component.OnSeasonDeactivated(oldSeason);

					if (arrival) {
						component.OnSeasonEnd(oldSeason);
					}
				}

				foreach (var component in newSeason.Components) {
					component.OnSeasonActivated(newSeason);

					if (arrival) {
						component.OnSeasonBegin(newSeason);
					}
				}
			}
		}
	}
}
