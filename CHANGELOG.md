# Navigation
(Click on version number to jump to its changelog.)

| Version									| Release Date |
| ----------------------------------------- | ------------ |
| [5.0 BETA 12](#50-beta-12)				| TBA		   |
| [5.0 BETA 11**F**](#50-beta-11f)			| `2022.09.11` |
| [5.0 BETA 11**E**](#50-beta-11e)			| `2022.06.30` |
| [5.0 BETA 11**D**](#50-beta-11d)			| `2022.06.20` |
| [5.0 BETA 11**C**](#50-beta-11c)			| `2022.06.06` |
| [5.0 BETA 11**B**](#50-beta-11b)			| `2022.06.04` |
| [5.0 BETA 11](#50-beta-11)				| `2022.06.02` |
| [5.0 BETA 10](#50-beta-10)				| `2022.05.18` |
| [5.0 BETA 9 **FIX 1**](#50-beta-9-fix-1)	| `2022.02.10` |
| [5.0 BETA 9](#50-beta-9)					| `2022.01.21` |
| [5.0 BETA 8](#50-beta-8)					| `2021.07.19` |
| [5.0 BETA 7](#50-beta-7)					| `2021.06.14` |
| [5.0 BETA 6](#50-beta-6)					| `2021.05.13` |
| [5.0 BETA 5 **FIX 1**](#50-beta-5-fix-1)	| `2021.04.29` |
| [5.0 BETA 5](#50-beta-5)					| `2021.04.29` |
| [5.0 BETA 4](#50-beta-4)					| `2021.04.11` |
| [5.0 BETA 3](#50-beta-3)					| `2021.03.20` |
| [5.0 BETA 2 **FIX 1**](#50-beta-2-fix-1)	| `2021.03.14` |
| [5.0 BETA 2](#50-beta-2)					| `2021.03.06` |
| [5.0 BETA 1](#50-beta-1)					| `2021.02.27` |

# 5.0 BETA 12

### ***THIS UPDATE IS A WORK IN PROGRESS***

### Additions
- Added "Critical Judgement" - A brand new counter mechanic that rewards players with a critical strike in a short time window after they avoid damage with a dodgeroll. It also turns your eye into a LED light.
- Added Bunnyhop + Dodgeroll combination - [#139](https://github.com/Mirsario/TerrariaOverhaul/pull/139).
- Added a tooltip to grappling hooks that hints that the grapple key can be held down for a classic pull.
- Added a feature of music playback position synchronization.
- Reimplemented boss death music pausing.
- Added tiny screenshakes for enemies getting hurt and killed.
- Reimplemented GoreStay from 1.3. Toggleable.
- Improved gore interaction, added splash force to bullet impacts.
- Added bullet & ice decals.
- Added a tiny toggleable tweak that causes the OS cursor to be displayed when interface is disabled.

### Changes
- Rewrote and improved everything about the camera features - [#168](https://github.com/Mirsario/TerrariaOverhaul/pull/168).
- Heavily improved melee swing velocity, now respects player movement input - [#137](https://github.com/Mirsario/TerrariaOverhaul/pull/137) (and many commits after that.)
- Enemy velocity no longer stacks indefinitely from knockback. Fixes shotguns launching demon eyes to space.
- Melee air combat completely reworked:
	- Now uses player movement input in place of attack direction. This is much more predictable, natural, and leaves the player in control.
	- No longer reduces knockback done to enemies while mid air, which had to be done due to the aforementioned knockback stacking.
	- The above changes result in the players now being able to keep themselves above enemies even by striking downwards, or even push themselves away from the enemies they're attacking. Experiment!
	- Now enabled on hammers, not sure why it wasn't.
- Reworked and rebalanced melee velocity-based damage:
	- Reduced world view bloat: hitting enemies no longer creates separate "combat text" lines.
	- Added a visual buff that shows the current effectiveness factor and the damage multiplier.
	- Rebalanced to straightforward and slightly nerf melee. Damage mltiplier now capped in `[1.0, 2.0]` range, mapped to player speed range of `[0.9, 12.0]`, all subject to changes.
	- Improved damage text coloring.
- Ledge climbing will now preserve player velocity, is now a tone more fluid.
- Added Bunnyhop Combos, reworked Bunny Paw - [#140](https://github.com/Mirsario/TerrariaOverhaul/pull/140).
- New health & mana pickup sounds - Closes [#113](https://github.com/Mirsario/TerrariaOverhaul/issues/113).
- Heavily improved life & mana pickup visuals.
- Broadswords' Killing Blows can no longer occur more than once per swing.
- Ambience: Improved bird & cricket audio.
- Ambience: Bird audio is now tied to the amount of nearby trees.
- Ambience: All booleans that dictated presence of ambience tracks are now replaced with smooth floating point factors.
- Increased the Mop's effectivity against gore.
- Made the mop use a texture for decal clearing. Now more effective, too.
- Decals will now be cleared from doors when they open.
- Decals will now be cleared from tiles that become surrounded (Might be inconssistent.)

### Compatibility
- Improved compatibility friendliness of grappling hook physics code.
- Fixed slashes appearing for `noMelee` weapons.
- Fixed melee animations not checking for `useStyle` in realtime.
- Fixed melee gore interaction not checking for `noMelee` boolean.
- Fixed HoldOut animations lingering on weapons that swap animation types.
- Fixed melee mana experiments breaking "AltUse shoot" weapons, like Thorium's `Valadium Slicer`.

### Fixes
- Fixed [#101](https://github.com/Mirsario/TerrariaOverhaul/issues/101) - Binoculars & other camera offsets do not work.
- Fixed [#119](https://github.com/Mirsario/TerrariaOverhaul/issues/119) - Smooth Camera doesn't handle teleports, reveals map areas.
- Fixed a rare load-time threading error.
- Fixed the Mop and a few other weapons not interacting with gore & gibs.
- Fixed explosions accelerating their own particles into bottom right direction.
- Fixed some vanilla grappling hooks not emitting lights and other effects after latching onto a tile.

### Configuration
- PR [#157](https://github.com/Mirsario/TerrariaOverhaul/pull/157) by **BAMcSH** - Added bool config to toggle air combat functionality.
- PR [#166](https://github.com/Mirsario/TerrariaOverhaul/pull/166) by **Hoabs** - Added config option to enable/disable difficulty tweaks.
- Added `Enemies.EnableEnemyLunges` option.

### Localization
- Localization files in the GitHub repository will now be refreshed based on English strings automatically on mod rebuild, making the development and localization contribution processes a lot more convenient.
- Main menu overlays are now localizable.
- Brazilian Portuguese - PR [#135](https://github.com/Mirsario/TerrariaOverhaul/pull/135) by **Pixelnando**.
- Chinese - PRs [#112](https://github.com/Mirsario/TerrariaOverhaul/pull/112), [#131](https://github.com/Mirsario/TerrariaOverhaul/pull/131) and [#170](https://github.com/Mirsario/TerrariaOverhaul/pull/170) by **Cyrillia**, PR [#130](https://github.com/Mirsario/TerrariaOverhaul/pull/130) by **ZHAI10086**.
- French - PR [#134](https://github.com/Mirsario/TerrariaOverhaul/pull/134) by **orian34**.
- German - PRs [#115](https://github.com/Mirsario/TerrariaOverhaul/pull/115) and [#133](https://github.com/Mirsario/TerrariaOverhaul/pull/133) by **Foxx-l** and **CriddleZap**.
- Polish - PRs [#141](https://github.com/Mirsario/TerrariaOverhaul/pull/141) and [#142](https://github.com/Mirsario/TerrariaOverhaul/pull/142) by **J00niper**.
- Russian - PRs [#138](https://github.com/Mirsario/TerrariaOverhaul/pull/138) and [#158](https://github.com/Mirsario/TerrariaOverhaul/pull/158) by **Snoop1CatZ69** and **Blueberryy**.
- Spanish - PRs [#120](https://github.com/Mirsario/TerrariaOverhaul/pull/120) and [#132](https://github.com/Mirsario/TerrariaOverhaul/pull/132) by **Wolf-Igmc4**.

# 5.0 BETA 11F

### Additions
- **(!)** Implemented configuration synchronization. Behavior of the mod will no longer revert to default settings when playing in multiplayer, and will follow the server's configuration file instead.
- PR [#122](https://github.com/Mirsario/TerrariaOverhaul/pull/122) by **clownwithnoname** - Added config toggles for melee power attacks.
- PR [#128](https://github.com/Mirsario/TerrariaOverhaul/pull/128) by **Tenrys** - Added config options for new Melee Animations and Slash sprites.
- Added `PlayerVisuals.EnableAimableFlashlights` config option.

### Fixes
- Issues [#151](https://github.com/Mirsario/TerrariaOverhaul/issues/151) and [#147](https://github.com/Mirsario/TerrariaOverhaul/issues/147) - Excessive mana drops with worms & segmented enemies. Segmented (worm-like) enemies now drop mana at random segments. This is the issue that resulted in some Calamity-ish bosses not dropping loot bags.
- Issue [#123](https://github.com/Mirsario/TerrariaOverhaul/issues/123) - Texture2D failure crash (Chunks being created outside the world.)
- Issue [#118](https://github.com/Mirsario/TerrariaOverhaul/issues/118) - Driver & blood related world texture corruption.
- Issue [#143](https://github.com/Mirsario/TerrariaOverhaul/issues/143) - Wings not pulling you up instantly.
- Issue [#136](https://github.com/Mirsario/TerrariaOverhaul/issues/136) - Hooks have incorrect ranges and can launch players.
- Issue [#117](https://github.com/Mirsario/TerrariaOverhaul/issues/117) - Prevent hook "drifting" while moving horizontally.
- Issue [#110](https://github.com/Mirsario/TerrariaOverhaul/issues/110) - AlwaysShowAimableWeapons option not being clientsided.
- Issue [#149](https://github.com/Mirsario/TerrariaOverhaul/issues/149) - Config button may crash the game.
- Fixed & improved audio filtering tests (unfortunately with assumptions.)
- Fixed tool PvE hitchecks running after animation ended.
- Fixed CanUseItem not being checked for power attacks. This slightly improves compatibility with modded content.
- Fixed segmented enemies bleeding excessively.
- Improved melee cooldown IL patches' stability.
- Fixed mount rotations getting stuck, re-enabled tilting at half intensity.
- Fixed killing blows applying to "projectile npcs".
- Fixed somehow typo'd discord invite in main menu.
- Try-catch for config FileSystemWatcher.

# 5.0 BETA 11E

### Additions
- Added `PlayerMovement.EnableAutoJump` config toggle.
- Added `PlayerMovement.EnableGrapplingHookPhysics` config toggle.

### Changes
- Rewritten melee weapon range calculation functions, now way more accurate. This is a bugfix, but it does directly affect gameplay.. Probably a buff.

### Fixes
- Fixed the mod not working on headless servers (you may need to be on preview TML until 1st of July.)
- Fixed dedicated servers crashing on 29/30th June TML preview after a super recent fix for illegal operations not being reported.
- Fixed many mod accessories that affect melee weapon size not doing so with Overhaul (Thanks, JaksonD!)

# 5.0 BETA 11D

### Changes
- Polished mana drops again. They will now be culled by the amount needed on per player basis.
- Adjusted volume of some ambience tracks, made broadsword swings a bit quiter.
- Changed `Accessibility.ForceAutoReuse`'s netside to `Both`.

### Fixes
- ❗❗❗ Fixed bosses sometimes not dropping their loot due to too many mana drops being spawned and overtaking all slots.
- Fixed URL link opening issues. I fixed this through TML, so make sure it's up to date too.
- Fixed [#104](https://github.com/Mirsario/TerrariaOverhaul/issues/104) (Grapples don't release when tile is mined.)
- Fixed ambience tracks getting stuck at low volume.
- Fixed Brand of Inferno's right click behavior.
- Fixed EnableProjectileSwordManaUsage option imperfections.

# 5.0 BETA 11C

### Additions
- Config files will now automatically reload when modified.
- Added `Accessibility.ForceAutoReuse` config option.

### Changes
- Compatibility - Excluded ClickerClass items from ForceAutoReuse.

### Fixes
- Issue [#103](https://github.com/Mirsario/TerrariaOverhaul/issues/103) - Fixed looping sounds being louder than supposed to.
- Issue [#103](https://github.com/Mirsario/TerrariaOverhaul/issues/103) - Fixed ManaRegenerationSoundVolume setting not working.
- Fixed the mod breaking in TML debug builds.
- Avoid injection & lock in ChunkLighting - Potential workaround for extremely rare lighting-related freezing issues.
- Fixed squirrel hooks not working on trees.

# 5.0 BETA 11B

### Additions
- Added `EnableProjectileSwordManaUsage` configuration toggle.
- Spanish difficulty level translations (Thanks, Igmc!)
- ru-RU grammar corrections (Thanks, Snoop1CattZ69!)
- Included a small version of the icon, may be used by other mods.

### Changes
- Public Overhaul builds will now also be uploaded to Github releases.

### Fixes
- Fixed spears & chain knives using mana.
- Made main menu links preferably open in steam for now, due to some issues in Firefox & Process.Start's UseShellExecute.

# 5.0 BETA 11

### News
- **Goodbye, MergedTesting.** Overhaul no longer needs custom TML builds, and is now compatible with both 1.4-stable (soon to be default) and 1.4-preview Steam branches.
- **Overhaul is now out on Steam Workshop!**
### Additions
- Reintroduced Monster Banner Rework from 1.3 Overhaul. The rework is the same as it was in 1.3, it replaces defense & damage bonuses with double loot & coin drops.
- Configuration files now store last used mod version, so that resets like the above could be prevented in the future.
- Added `EnableAimingRecoil` ConfigEntry, aiming recoil is now disabled by default due to instability and its experimental nature.
- Added a Terraria Overhaul Music Pack getting/enabling button to main menu. Currently not ideal however, as Steam won't download this vanilla Terraria pack until you close TML. Also, doesn't do anything for GOG users. So I'll probably be upgrading this later.
- Added a `Configuration` button to main menu. Currently, it just opens the `Config.json` file in whatever your default json editor is.
### Fixes
- Fixed an issue with config files that resulted in screenshake strength and other floating point values to be zero by default. Because of this, configs will now be reset.
- Issue [#82](https://github.com/Mirsario/TerrariaOverhaul/issues/82) - Difficulty names go out of bounds in GUI - Fixed partially.
- Issue [#95](https://github.com/Mirsario/TerrariaOverhaul/issues/95) - Dodgeroll UI sounds are global in MP.
- Fixed gun recoil being shared in MP.

# 5.0 BETA 10

### Additions
- Mana regeneration is now based on player speed. Complete with a beautiful indicator in the top left. The already present mana regeneration sounds compliment this too.
- Added 4 configuration options related to low health, low mana, and mana regen effects.
- Brand new logo by Zoomo & Donmor. I've also added some glowmasks to the one in main menu, so it looks better in dark.

### Changes
- Improved dodgerolls with twice the dodgerolls. Players now have two dodgeroll charges, for which polished indicators have been added. Using up all charges before they're restored penalizes players with a longer cooldown.
- Weapons' power attacks' activation no longer requires a new button press, and it's all now auto-reusable.
- Health & Mana drops now exist per-player in multiplayer.
- Health drop rules updated. 2 hearts will now be dropped at >25% and <=33% player health, instead of just 3 drops getting replaced with 1 immediately after the player reaches health above 25%.
- Statue-spawned enemies now drop health too.
- Health and mana default pickup ranges increased:
	- 10.5 -> 11 blocks for health;
    - 12 -> 16 blocks for mana.
- Large codebase quality improvements, once again. Is gud!

### Fixes
- Fixed many multiplayer synchronization issues! Multiplayer experience should now be quite comfortable.
- Issue [#94](https://github.com/Mirsario/TerrariaOverhaul/issues/94) - Enemy mana drops don't accumulate from debuff damage.
- Issue [#92](https://github.com/Mirsario/TerrariaOverhaul/issues/92) - Add option to turn off the low hp sounds.
- Issue [#91](https://github.com/Mirsario/TerrariaOverhaul/issues/91) - Boomerangs have reversed knockback.
- Issue [#89](https://github.com/Mirsario/TerrariaOverhaul/issues/89) - Audio processing isn't always supported, and may crash the game.
- Issue [#84](https://github.com/Mirsario/TerrariaOverhaul/issues/84) - Gun muzzleflashes incorrectly account for Item.scale.
- Issue [#73](https://github.com/Mirsario/TerrariaOverhaul/issues/73) - Climbing while using Bunny Paw cause Player char stuck on jumping animation.
- Issue [#61](https://github.com/Mirsario/TerrariaOverhaul/issues/61) - ChildSafety enemy death particles can bleed.
- Issue [#59](https://github.com/Mirsario/TerrariaOverhaul/issues/59) - Can't sit, can't sleep (Thanks to JaksonD for the fix!)
- Issue [#57](https://github.com/Mirsario/TerrariaOverhaul/issues/57) - Enemies that spawn enemies on death spawn more than they should.
- Issue [#54](https://github.com/Mirsario/TerrariaOverhaul/issues/54) - Healing hearts and mana stars don't spawn in multiplayer.
- Issue [#53](https://github.com/Mirsario/TerrariaOverhaul/issues/53) - SDMG keeps muzzleflash after being fired.
- Issue [#50](https://github.com/Mirsario/TerrariaOverhaul/issues/50) - Sword power attacks not shown to other players.
- Fixed chunk textures being loaded outside the main thread (that caused rare crashes.)
- Fixed bullet/shell casings & arrow chunks turning into mist when the vanilla Blood & Gore option is turned off.
- Fixed broadswords' killing blows' damage multipliers not making their way to multiplayer servers, causing "killed" enemies to come back.

### Localization
- Updated Chinese localization (Thanks, Cyril!)
- Updated Brazilian Portuguese localizations (Thanks, Pixelnando!)

# 5.0 BETA 9 FIX 1

### Fixes
- Fixed the mod not working on servers.

# 5.0 BETA 9

### Additions
- Difficulty levels have finally been (partially) overhauled, with names again turned idiosyncratic (why wouldn't they be) and descriptions updated.
Debuff lengths, enemy health, defense, knockback, and coin drops are now the same between the 3 (4) difficulty levels, equalized mostly around Expert's values.

	Enemy-to-Player damage multipliers switched from `[1.0, 2.0, 3.0]` to `[1.25, 2.0, 3.25]`.

	Here are the new difficulty level names & descriptions:
	- **Journey** - `The Creative / Accessibility mode (For those who want to be in control)`
	- **I'm Too Young To Mine** (Normal) - `Less damage from enemies, but same rewards (For the easy-going)`
	- **Bone Me Plenty** (Expert) - `The new standard experience (For those who like a fair challenge)`
	- **I Am Cthulhu Incarnate** (Master) - `Extreme punishment and new rewards (For the truly ruthless)`

	(In the future, Expert's features will be backported to "Normal", with slightly less severity)
- **Experimental:** Projectile-firing swords now use 3-10 mana, depending on their firerate. I already have another idea for them, but let me know what you think, and don't forget the above 2 points before writing hate mail.
- Axes & Hammers now have charged attacks (not fully done with them.)
- Axes & Hammers now use broadswords' swing animations.

### Changes
- Huge backend rewrites - now component-based with no reliance on the unreliable paradigm that is inheritance.
- Mana drop logic redone again. The amount of mana dropped from each enemy is now pre-determined based on their assumed (by the mod) role. Fodder will drop `15` mana, heavies - more, bosses - depending on their healthpools. The used weapon's stats no longer affect mana drops at all. Honestly not sure why they did, it's pretty dumb.
- Mana regeneration redone & rebalanced again. Regeneration is now (again???) constant and works even while firing. The vanilla "staying still doubles mana regen" feature has been (again????) removed. A multiplier to the whole regen value has been added, to prevent regeneration buffs & accessories from being too overpowered.
- Life & Mana pickups now disappear in `10` seconds instead of `5`.
- Brand new broadsword audio! Again! (Thanks egshels!)
- Brand new audio for tools! (No thanks to egshels!)
- Melee weapons' projectiles are now sped up when charged. Much more satisfying.
- Reduced Killing Blow damage scaling from `2.0` to `1.5`.
- Reduced Magic Weapon charged projectile damage multiplier from `2.50` to `1.75`.

### Fixes
- Fixed killing blows only doubling damage on kills that would happen regardless. Facepalming.
- Fixed slashes showing up in after-images (appearing not translucent during dashes.)
- Space Gun will no longer be considered a MagicWeapon. An overhaul for it will be reimplemented soon.

# 5.0 BETA 8

### Additions
- Replaced magic staves' sound effect with an actually good one.
- Added a charged magic blast sound effect.
- Added screenshake to magic power attack charges.

### Changes
- Made bow sounds quieter.
- Simplified mana drops. No longer exclusive to magic damage, no longer based on the current weapon's mana usage.
- Mana is no longer regenerated during item use (like it wasn't in vanilla.) This is done because you're now able to get mana back from enemies.
- Readjusted melee velocity boosts once more to battle abuse.
- Rebalanced magic power attacks, made them quicker.

### Fixes
- Fixed magic weapon power attacks not checking for mana until fire.
- Fixed broadsword charged attacks being activatable during item use.
- Fixed broadswords' power attacks not being reset on item reuse.
- Fixed various textures being disposed on worker threads, causing crashes on unloading.
- Fixed dodgerolls being able to be activated when hovering over GUI.
- Fixed mana effects being active while dead.
- Fixed Main Menu links not working (by RighteousRyan.)

### Localization
- Brazilian Portuguese localization (by goiabae & Pixelnando.)
- Polish Localization (by J00niper.)

# 5.0 BETA 7

### Additions
- Implemented a new configuration design.
- Implemented 12 new configuration options, for a total of 14. (Welcoming code contributions for this! See issue [#41](https://github.com/Mirsario/TerrariaOverhaul/issues/41).)
- Implemented Localizations. For now, translations include Spanish and Russian. (If you want to contribute, see GitHub.)
- Early bow overhaul. Just audio & visuals.
- Added a "fan the hammer" alt fire to revolvers.
- Reworked miniguns, all now have star cannon's recoil velocity, i.e. you can hover by shooting downwards! (Send feedback.)
- Implemented gore footsteps via the new physical material system.
- Added jump footstep sounds to dirt, grass, stone and wood.
- Wooden arrows now break into visible 'gore' pieces.
- Implemented a basis for seasons. They don't actually appear in the game yet.

### Changes
- Lowered overall aim recoil.

### Fixes
- Fixed weapon rotations jumping when switching direction.
- Fixed the slash draw layer possibly rendering when charging.
- Fixed some issues in player direction code.

# 5.0 BETA 6

### Additions
- Redid player physics. Now more vanilla-like, with vanilla jump height and more air control. Jump key holding physics has been added back and rewritten, and jumps are way smoother than in vanilla.
- Experimental: Added gun recoil. Let me know what you think after trying it out. In the future, this will let me remove random spread from many guns, like the miniguns.
- Reimplemented the audio ambience system. Currently has 4 audio tracks for 3 different biomes.
- Implemented low pass filtering-based wall occlusion of outdoors ambient sounds. You will no longer hear birds so clearly while indoors or in a cave.

### Changes
- New grass, dirt and stone footstep sounds that I find less jarring.

### Fixes
- Fixed "projectile enemies" being able to drop health and mana.
- Fixed the ChunkSystem sometimes freezing on mod unload.

# 5.0 BETA 5 FIX 1

### Fixes
- Fixed the mod's code injections crashing on TML release configuration.

# 5.0 BETA 5

### Additions
- Reimplemented FNA audio processing, which, in <=4.X, was only available on unofficial TML versions.
- The reverb, which was previously just based on the local player's depth in the world, is now completely dynamic and surroundings based.
- Introduced experimental sound occlusion, which will do 'line of sight' checks towards the player on every sound. Sounds quite good with giant worms.
- Being underwater will muffle sounds in a bit smoother way than before, and with a better sound playing.
- Explosions near the player will muffle out audio for a bit of time.
- Introduced new low health effects. They muffle out not just audio, but also music. **NOTE:** Applying low-pass filtering to music requires Overhaul's music pack, or other music that comes from a resource pack or a mod.
### Changes
- Nerfed health drops from enemies.
- Explosions' decals' alpha is now based on the guessed explosion power.
### Fixes
- Fixed the mod crashing on servers. Multiplayer is still quite unstable.
- Fixed [#49](https://github.com/Mirsario/TerrariaOverhaul/issues/49) - Combat crosshair impulse triggered by all players.
- Fixed [#46](https://github.com/Mirsario/TerrariaOverhaul/issues/46) - Crosshairs spread out when switching to gun after melee attacking.
- Hardcoded a fix for explosive bullets having too powerful effects.

# 5.0 BETA 4

### Additions
- Completely reworked mana to try and make magic weapons at least somewhat viable without potion spamming & constant standing still.
- Reimplemented and rebalanced health drop changes.
- Increased default pickup range
- MagicWeapon overhaul - screenshake and power attacks.
- Reimplemented crosshairs.
- Added outlines to the crosshair.
- Implemented low mana warning effects & a sound.
- Implemented mana regeneration effects & a sound.
- Added 'Mana Channelling' UX buff to notify the player that standing still speeds up mana regeneration.

### Changes
- Heavily speeded up climbing.

### Fixes
- Fixed autoreuse forcing breaking magic missile & other items.
- Fixed 1.4.2 TML getting stuck due to an unused corrupted ogg.
- Fixed [#43](https://github.com/Mirsario/TerrariaOverhaul/issues/43) - "If using a charged melee attack and it won't end before dying, then after respawning it will be triggered"
- Fixed power attacks being triggered when interacting with tiles.

# 5.0 BETA 3

### Additions
- Added broadsword power attacks.
- Added broadswords killing blows. They're very different from 1.3 -- they double damage on power attacks whenever an enemy with non-full health will die from that.
- Striking enemies will now launch the player towards their velocity. This lets you 'ride' demon eyes into the sky!
- Made a new system that allows me to replace enemy hit sounds on per-item basis. Swords now play custom audio, and so do wooden tools.

### Changes
- Redid swing velocity logic from scratch. Removed extra velocity bonuses for attacking downwards.
- Heavily improved enemy hit & death sounds.
- Heavily improved gore audio and fixed some issues with it.
- Improved gore blood splatters.
- Removed unused keybindings.

### Fixes
- Fixed configs not loading.
- Fixed retro lighting lagging the game (and sometimes crashing on some graphics drivers.)
- Improved the mop's code, fixed the mop hitting enemies through walls.


# 5.0 BETA 2 FIX 1

### Fixes
- Fixed servers not working due to client things autoloading.
- Fixed the mop hitting through walls.
- Moved keybind definitions, removed currently-unused ones.
- Fixed configs not loading.

# 5.0 BETA 2

### Additions
- New melee animations.
- Directional knockback for NPCs.
- Striking with melee weapons now lets the player stay up in the air.
- New arc-based melee collision checks.
- Removed damage randomization.
- Velocity-based melee damage.
- New perfected muzzleflashes for guns.
- Melee screenshake.
- Broadsword attack dashes, with many adjustments.
- Added new NPC hit effects: scaling, rotation & offsets.
- Reimplemented npc "stuns" for melee.
- Added debug mode - /oToggleDebugDisplay.

### Changes
- Lowered footstep volume.

### Fixes
- Fixed gun recoil going in the wrong direction when gravity is flipped.
- Fixed the player rotating incorrectly when gravity is flipped. 

# 5.0 BETA 1

This is the first public release of version 5.0, which is a from-scratch recreation of the mod.

### Starting features
- (!) Jump key buffering and a new addition: forced-on auto-jump.
- (!) New and improved NPC blood and gore.
- (!) Improved grappling hook swinging physics.
- (!) New and improved decals.
- (!) Now-automatic gun overhauls with improved audio, screenshake, and bullet/shell casings.
- Camera improvements.
- Footstep sounds.
- Movement changes.
- Bunnyhopping.
- Players facing the mouse cursor.
- Velocity-based rotation for players.
- Walljumps and wallrolls.
- Climbing.
- Melee weapons audio.
- Other things.