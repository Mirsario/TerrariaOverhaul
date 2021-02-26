# What's this?
Terraria Overhaul is a huuuge game mechanics changing mod for Terraria. You can read more about it at [this forum page](https://forums.terraria.org/index.php?threads/.60369/).

# What's the state of the rebuild?
So far, only the most basic features have been reimplemented. Including, but not limited to:
- Camera improvements.
- Footstep sounds.
- Movement changes.
- Bunnyhopping.
- Jump key buffering & auto-jump.
- Players facing the mouse cursor.
- Velocity-based rotation for players.
- Walljumps and wallrolls.
- Climbing.
- NPC Bleeding.
- Grappling hook swinging physics.

They still, however, make a big impact on the game's feel.

# How do I build this?

### For people who know what they're doing:
With the '[1.4_mergedtesting](https://github.com/tModLoader/tModLoader/tree/1.4_mergedtesting)' branch of tModLoader.

### For people who don't know what they're doing:
Below is a step by step instruction for that. It only assumes that you know at least how to use cmd. You should.

**Getting the right tModLoader:**
- You need Windows to build it.
- Install [Visual Studio 2019 Community](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&rel=16) (It's free.)
During installation, check the `.NET desktop development` workload
- Install Git if you don't have it - [Git For Windows](https://git-scm.com/download/win).
- [Install .NET Core 3.1 developer pack](https://dotnet.microsoft.com/download/visual-studio-sdks).
- Clone the 1.4_mergedtesting branch - `git clone https://github.com/tModLoader/tModLoader -b 1.4_mergedtesting`.
- Run setup.bat and click Setup, pointing the tool to your latest vanilla terraria .exe.
- Run `solutions/buildDebugAndMac.bat`. This will build 1.4 tML as 'tModLoaderDebug.exe' in your tML directory.

**Compiling the mod:**
- Clone the mod's 1.4 branch into `%userprofile%/Documents/My games/Terraria/ModLoader/Beta/Mod Sources`. Note the Beta directory.
The git command for that would be `git clone https://github.com/Mirsario/TerrariaOverhaul -b 1.4`.
- Head over to Mod Sources in-game and `Build & Reload` TerrariaOverhaul.

That's all. Use `git fetch` and `git pull` to keep up with updates. Note that you'll need to rerun patching in the tML setup tool when updating tML.

## License
All code of this repository is provided under [the MIT License](https://github.com/Mirsario/TerrariaOverhaul/blob/1.4/LICENSE.md).

All of the art, audio, and other non-code assets belong to their respective owners and should not be copied and distributed without getting a permission or a license from them.
