// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

AmbienceTrack: {
	Sound: {
		SoundPath: "TerrariaOverhaul/Assets/Sounds/Ambience/Beach/BeachWaves"
		Type: Ambient
		Volume: 0.33
		IsLooped: true
	}

	Variables: {
		Volume: {
			Inputs: [ "Beach", "SurfaceAltitude", "NotUnderwater" ]
			Operation: Multiply
		}
		Position: {
			Inputs: [ "Water" ]
			Operation: Average
		}
	}

	Positional: {
		Range: {
			Start: 128.0,
			End: 2048.0,
			Exponent: 2.0,
		}

		Tiles: [
			"Torches",
			"Campfire",
			"Furnaces",
			"Fireplace",
		]
	}
}
