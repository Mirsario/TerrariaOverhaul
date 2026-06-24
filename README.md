<p align="center">
	<img src="https://github.com/Mirsario/TerrariaOverhaul/blob/dev/Logo_Header.png?raw=true"/>
</p>

- [**Homepage** ⌚](https://mirsar.io/Projects/TerrariaOverhaul)
- [**Discord** 💬](https://discord.gg/RNGq9n8)
- [**Steam** 📦](https://steamcommunity.com/sharedfiles/filedetails/?id=2811803870)
- [**Wiki** 📝](https://mirsar.io/TerrariaOverhaul/Wiki)

# 🔥 What's this?
Terraria Overhaul is a massive mod for Terraria that focuses on increasing overall enjoyment of the game, via improving aspects such as: combat looks, feel, & mechanics, movement, camera, ambience, and even the entire soundtrack.

# ⌚ What's the state of the rebuild?
This version is around 80%+ done.
It's missing many features from the 1.3 version, but what's currently implemented is much superior to the old versions.

[Click here to see a mostly-full comparison list for feature parity between v4.x and v5.0.](https://github.com/Mirsario/TerrariaOverhaul/issues/108)

# ⚙️ How do I build this?

Below is a step by step instruction for that. It only assumes that you know at least how to use command prompts, shells, or terminals. You should.

- Get TModLoader from [Steam](https://store.steampowered.com/app/1281930/tModLoader) or [GitHub](https://github.com/tModLoader/tModLoader/releases).
- Get Git from [git-scm](https://git-scm.com/download) or from a Linux package manager. Most defaults suffice in the installer.
- Get the [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) SDK.
- Clone the mod into:
  - Windows: `%userprofile%/Documents/My games/Terraria/tModLoader/ModSources`
  - Linux: `~/.local/share/Terraria/tModLoader/ModSources`
  - MacOS: `~/Users/USERNAME/Library/Application Support/Terraria/tModLoader/ModSources`
The git command for that would be `git clone https://github.com/Mirsario/TerrariaOverhaul -b dev`, where `dev` is the branch you want to clone.
- Only needed once: Open TModLoader, and navigate to the `Workshop -> Develop Mods` screen, so that TML generates an important targets file. Then you just close it.
- Build the mod by running `dotnet build -c Release` in the cloned folder. Omit `-c Release` if you are developing or want to debug issues. 

That's all. Use `git pull` to pull new commits, and `git reset origin/dev --hard` to force-reset your local repository.


# 📖 License
#### Code
All code files in this repository have license headers.<br/>
Most of the original code being provided under the [GNU General Public License 3.0](https://github.com/Mirsario/TerrariaOverhaul/blob/dev/LICENSE.md).<br/>
Exceptions: `Utilities/FastNoiseLite.cs` (MIT).
#### Assets
All of the art, audio, and other non-code assets belong to their respective owners and are used non-commercially either with a permission, a royalty-free license, or with various modifications & trust in the holiness of videogame modding.
No copyright infringements and no commercial use are intended.

# ❤️ Contributing
Thank you to the following contributors for helping improve the mod for everyone!

<a href="https://github.com/Mirsario/TerrariaOverhaul/graphs/contributors">
	<img src="https://contrib.rocks/image?repo=Mirsario/TerrariaOverhaul&max=900&columns=20" />
</a><br/><br/>

If you would like to contribute, first of all thank you, and as for second, please read the following:

- This is a creative human-made work, created as a skill showcase among other things. Please respect the effort of prior contributors, and make your code and/or assets on your own, without use of agentic AI. If you have any questions about the codebase - do not be afraid to ask.

- The [`#development`](https://discord.gg/RNGq9n8) Discord channel is public for all, feel free to ask anything there.
- Namespaces are limited to 3 elements, counting the root.
  - `ModName.Core.(Area)` contains engine-like functionality, usually mod-agnostic.
  - `ModName.Common.(Area)` contains general mod logic, including that which is meant to be used by content.
  - `ModName.Content.(Area)` contains game content, be it JSON-like data or TML-style code.
  - `ModName.Utilities.(|XNA|Terraria)` contains utilities organized by their dependencies, always meant to be mod-agnostic.
  - The area does not organize code files by what types they contain, but what game design area they represent, or which `Common` concept they accompany (e.g. the mop being placed in `Content/Decals`).
- Code files are split and merged based on the heuristic of "would X ever be of use without Y?". Large code files are good!
- Build the mod before making commits that contain content changes, in order to run the localization file regeneration task.
- Respect the whitespace, make sure that your editor does not introduce `tabs vs spaces` and `\n vs \r\n` fights.
