<p align="center">
  <img src="https://github.com/Mirsario/TerrariaOverhaul/blob/1.4/Content/Menus/Logo.png?raw=true" alt="Sublime's custom image"/>
</p>


# What's this?
Terraria Overhaul is a *huuuge* game mechanics changing mod for Terraria.
You can read more about it at [this forum page](https://forums.terraria.org/index.php?threads/.60369/) (Might be outdated.)

# What's the state of the rebuild?
This version is around 60%+ done.
It's missing many features from the 1.3 version, but what's currently implemented is much superior to the old version.

[Click here to see a mostly-full comparison list for feature parity between v4.x and v5.0.](https://github.com/Mirsario/TerrariaOverhaul/issues/108)


# How do I build this?

## For people who know what they're doing:
With the '[1.4_mergedtesting](https://github.com/tModLoader/tModLoader/tree/1.4_mergedtesting)' branch of tModLoader.

## For people who don't know what they're doing:
Below is a step by step instruction for that. It only assumes that you know at least how to use cmd. You should.

### Getting the right tModLoader

**THE EASY ROUTE:**
> - Get [TModLoader from Steam](https://store.steampowered.com/app/1281930/tModLoader/).

**THE HARD ROUTE:**
> - You need Windows to build it.
> - Install [Visual Studio 2022 Community](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022) (It's free.)
> During installation, check the `.NET desktop development` workload
> - Install Git if you don't have it - [Git For Windows](https://git-scm.com/download/win).
> - Ensure that you have .NET 6.0.1+ SDKs.
> - Clone the branch you want, be it `1.4` (dev), `1.4-preview` (preview), or `1.4-stable` (stable):
`git clone https://github.com/tModLoader/tModLoader -b branchname`.
> - Run setup.bat and click Setup, pointing the tool to your latest vanilla terraria .exe.
> - Run `solutions/buildRelease.bat`. This will build 1.4 tML into a `tModLoaderDev` directory, find it next to Terraria's and use 'start-tModLoader.bat' to launch it.

### Compiling the mod
> - Clone the mod's 1.4 branch into `%userprofile%/Documents/My games/Terraria/tModLoader/ModSources`. Note that the directory was recently updated.
The git command for that would be `git clone https://github.com/Mirsario/TerrariaOverhaul -b 1.4`.
> - Head over to Mod Sources in-game and `Build & Reload` TerrariaOverhaul.

That's all. Use `git fetch` and `git pull` to keep up with updates. Note that you'll need to rerun patching in the tML setup tool when updating tML.

## License
All code of this repository is provided under [the MIT License](https://github.com/Mirsario/TerrariaOverhaul/blob/1.4/LICENSE.md).

All of the art, audio, and other non-code assets belong to their respective owners and are used non-commercially either with a permission, a royalty-free license, or with various modifications & trust in the holiness of videogame modding.
No copyright infringements intended.

Besides for that, I only want to say - be nice, and please [contribute configuration](https://github.com/Mirsario/TerrariaOverhaul/issues/41) instead of splitting the mod's features into other mods. The latter is dickish.
