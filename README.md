<p align="center">
	<img src="https://github.com/Mirsario/TerrariaOverhaul/blob/dev/Content/Menus/Logo.png?raw=true"/>
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
- Get .NET 6 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
- Clone the mod into `%userprofile%/Documents/My games/Terraria/tModLoader/ModSources`.
The git command for that would be `git clone https://github.com/Mirsario/TerrariaOverhaul -b dev`, where `dev` is the branch you want to clone.
- Build the mod by running `dotnet build` in the cloned folder.

That's all. Use `git pull` to pull new commits, and `git reset origin/dev --hard` to force-reset your local repository.

# 📖 License
All code of this repository is provided under [the MIT License](https://github.com/Mirsario/TerrariaOverhaul/blob/dev/LICENSE.md).

All of the art, audio, and other non-code assets belong to their respective owners and are used non-commercially either with a permission, a royalty-free license, or with various modifications & trust in the holiness of videogame modding.
No copyright infringements intended.

Besides for that, I only want to say - be nice, and please [contribute configuration](https://github.com/Mirsario/TerrariaOverhaul/issues/41) instead of splitting the mod's features into other mods. The latter is dickish.

# ❤️ Contributors
Thank you to the following contributors for helping improve the mod for everyone!

<a href="https://github.com/Mirsario/TerrariaOverhaul/graphs/contributors">
	<img src="https://contrib.rocks/image?repo=Mirsario/TerrariaOverhaul&max=900&columns=20" />
</a>
