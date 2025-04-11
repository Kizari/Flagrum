# Format Information

### Ebony Archive (*.earc)

The main archive format for Luminous Engine games. Stores game files and various metadata about them.

### Ebony Replace (*.erep)

This format was introduced with Forspoken and is not used in FFXV. This file uses a hash table to tell the game
to replace existing files with replacement files.

---

## Final Fantasy XV Binary Mod (*.ffxvbinmod)

An Ebony Archive with extra copyguard protection designed for use with the Steam Workshop mod system
that Square Enix created. Unfortunately this system is very broken and mediocre, so it would be best
to not put any further effort towards it and instead focus on moving its functionality over to EARC mods.

These archives (usually) contain a few files with unexpected formats.

### Mod Metadata (*.modmeta)

An `ini` file that contains information about the mod, such as name, description, and stats.

### PNG Binary (*.png.bin)

Literally just a PNG file. The counterpart with the *.png extension is actually a BTEX file.