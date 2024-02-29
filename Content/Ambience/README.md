# Ambience Tracks

Files with the '.prefab.hjson' extension under this folder declare ambient sounds that should appear in the game.
'.prefab.hjson' files use the HJSON syntax: https://hjson.github.io

## Contributing

If you would like to contribute something to the ambience module, please note that:
- The source of the used sound effects must be known.
- Every sound effect should use the .OGG file extension & format.
- Every sound effect must be of high quality. Taking things from freesound.org-type websites is not going to cut it.

## Example File

Here's an example ambience track with comments over every line:

```cs
// 'Birds' is the name of this data blob and thus of the ambient track it contains. It should be unique across all prefab files.
Birds: {
	// AmbienceTrack component, unsurprisingly, stores all data relating to ambience tracks.
	// Do not rename this.
	AmbienceTrack: {
		// The 'Sound' property contains everything about how a sound should be played.
		// Inside this object, you can use any public non-span property you can find here:
		// https://github.com/tModLoader/tModLoader/blob/1.4.4/patches/tModLoader/Terraria/Audio/SoundStyle.TML.cs?ts=4#L43
		Sound: {
			// Path to the sound asset, including the mod's internal name and excluding file extensions.
			// Also excluding variants' number-suffixes.
			SoundPath: "TerrariaOverhaul/Assets/Sounds/Ambience/Forest/ForestBirds"
			// Type determines what volume slider should this track's volume be affected by.
			// Leave it at "Ambient" for every ambience track.
			Type: "Ambient"
			// A fixed volume multiplier.
			Volume: 0.45
			// Whether this sound should be looped.
			// Use 'true' if it's ambience like rain, use 'false' if it's a randomly triggered sound.
			IsLooped: true
		}

		// The 'Variables' property is used to calculate [0..1] floating point values (like a track's volume)
		// by using "environmental signals" and creating temporary variables.
		//
		// In this exact example, we create a temporary 'NotInExcludedBiomes' variable that will be set to the
		// inverse of the maximum value out of intensity of all biomes where birds shouldn't be heard.
		//
		// This variable is then used in a final 'Volume' variable calculation that multiplies 'NotInExcludedBiomes' with many other factors,
		// like the 'DayTime' signal that approaches 1.0 during the day and 0.0 during the night, or 'TreesAround', which makes
		// this sound's volume tied to the amount of trees that the player is near, quieting it down in areas clear of nature.
		//
		// Every classic AmbienceTrack should have a 'Volume' variable entry, or otherwise it'll never be heard.
		Variables: {
			// Calculates a temporary variable named 'NotInExcludedBiomes' as '1.0 - Max(Corruption, Crimson, Jungle, Desert, Tundra)'
			NotInExcludedBiomes: {
				// Denotes what signals & variables go into the calculation.
				Inputs: [ "Corruption", "Crimson", "Jungle", "Desert", "Tundra" ]
				// "Max" operation chooses the highest value out of all inputs.
				Operation: "Max"
				// "Inverse" modifier subtracts the semi-final result from 1.0.
				Modifiers: "Inverse"
			}
			// Calculates 'Volume' as 'NotInExcludedBiomes * DayTime * SurfaceAltitude * NotRainWeather * TreesAround'
			Volume: {
				// Denotes what signals & variables go into the calculation.
				Inputs: [ "NotInExcludedBiomes", "DayTime", "SurfaceAltitude", "NotRainWeather", "TreesAround" ]
				// "Multiply" performs a multiplication of all inputs.
				Operation: "Multiply"
			}
		}
		
		// If this is set to true - this sound will be muffled whenever the player is in front of a background wall.
		// Use this for "outdoors" sounds like birds & crickets.
		// If not specified - defaults to false.
		SoundIsWallOccluded: true
	}
}
```

## Environmental Signals

The following "environmental signals" are available for consumption in `Variables.*.Inputs` properties:

### Time
- `DayTime` - Starts approaching 1.0 in the morning, goes back to 0.0 in the evening.
- `NightTime` - Starts approaching 1.0 in the evening, goes back to 0.0 in the morning.
### State
- `Underwater` - Approaches 1.0 when the local player is underwater, to 0.0 otherwise.
### Altitude
- `SurfaceAltitude` - 1.0 when on surface, approaches 0.0 underground, in skies, or at extreme mountain peaks.
- `SurfaceOrSkyAltitude` - Like `SurfaceAltitude`, but flying upwards doesn't zero-out it until the player reaches space.
- `UnderSurfaceAltitude` - 1.0 when underground, approaches 0.0 near surface.
- `SpaceAltitude` - Determines whether the player is in space or close to it.
### Weather
- `RainWeather` - Scales depending on the rain's intensity, maxing out around halfway through.
- `NotRainWeather` - The inverse of `RainWeather`.
### Nature
- `TreesAround` - Approaches 1.0 depending on the amount of trees the player is next to.
- `TreesNotAround` - The inverse of `TreesAround`.
### Biome
**Floating Points:**
- `Corruption` - Affected by the amount of corruption blocks near the player.
- `Crimson` - Affected by the amount of crimson blocks near the player.
- `Hallow` - Affected by the amount of hallow blocks near the player.
- `Desert` - Affected by the amount of sand blocks near the player.
- `Jungle` - Affected by the amount of jungle grass blocks near the player.
- `Tundra` - Affected by the amount of snow and ice blocks near the player.
**Booleans:**
The following signals are determined by the vanilla game and are always either 1.0 or 0.0, with no values in-between. This may be changed in the future.
- `Dungeon`
- `Meteor`
- `WaterCandle`
- `PeaceCandle`
- `TowerSolar`
- `TowerVortex`
- `TowerNebula`
- `TowerStardust`
- `Glowshroom`
- `UndergroundDesert`
- `SkyHeight`
- `Beach`
- `Rain`
- `Sandstorm`
- `OldOneArmy`
- `Granite`
- `Marble`
- `Hive`
- `GemCave`
- `LihzhardTemple`
- `Graveyard`