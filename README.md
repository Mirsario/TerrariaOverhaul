<p align="center">
	<img src="https://github.com/Mirsario/TerrariaOverhaul/blob/dev/Logo_Header.png?raw=true"/>
</p>


# 🔥 What's this?
Terraria Overhaul is a *huuuge* game mechanics changing mod for Terraria.
You can read more about it at [this forum page](https://forums.terraria.org/index.php?threads/.60369/) (Might be outdated.)

# ⌚ What's the state of the rebuild?
This version is around 60%+ done.
It's missing many features from the 1.3 version, but what's currently implemented is much superior to the old versions.

[Click here to see a mostly-full comparison list for feature parity between v4.x and v5.0.](https://github.com/Mirsario/TerrariaOverhaul/issues/108)

# ⚙️ How do I build this?

Below is a step by step instruction for that. It only assumes that you know at least how to use command prompts, shells, or terminals. You should.

- Get TModLoader from [Steam](https://store.steampowered.com/app/1281930/tModLoader) or [GitHub](https://github.com/tModLoader/tModLoader/releases).
- Get Git from [git-scm](https://git-scm.com/download) or from a Linux package manager. Most defaults suffice in the installer.
- Get the [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) SDK.
- Clone the mod into `%userprofile%/Documents/My games/Terraria/tModLoader/ModSources`.
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

# ❤️ Contributors
Thank you to the following contributors for helping improve the mod for everyone!

<a href="https://github.com/Mirsario/TerrariaOverhaul/graphs/contributors">
	<img src="https://contrib.rocks/image?repo=Mirsario/TerrariaOverhaul&max=900&columns=20" />
</a>
