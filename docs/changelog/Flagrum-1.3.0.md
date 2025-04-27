## Flagrum 1.3.0

_Released 26 November 2022_

### New Features

- Added Chinese language support (huge thanks to Chisa for providing the translations)
- Flagrum now supports its own custom FMOD format, which allows for smaller mods,  
  quicker installation, and installation by double-clicking the mod without needing to launch Flagrum first
- Mods can now be favourited to keep important mods near the top of the list
- Mods are now sorted alphabetically by name
- Mods now have an optional README that allows for packaging rich text information in with your mod
- Mods now have categories, and mod cards can be filtered by category
- Files and earcs can now be navigated to quickly from the mod build list by clicking the button next to the path
- Custom key bindings can now be set on the 3D model viewer in the Asset Explorer  
  —thanks to cvlnomen for writing the code for part of this functionality
- Your own mods will now be cached on build to allow for faster build times on larger mods.  
  A button has been provided to clear this cache for a given mod
- You can now enter a URI or file path in the address bar of the Asset Explorer to navigate it quicker
- BTEX converter has been upgraded to support replacing any possible image type in the game

### Changes

- Exporting a mod will now export to FMOD instead of ZIP
- Mod card information has been separated from the build list to allow for  
  editing mod information without rebuilding a mod

### Bug Fixes

- Mod builder will no longer corrupt game files in the rare case that Flagrum crashes during build
- Backup files will no longer be missing in the rare case that Flagrum crashes during mod build
- Fixed issue where large missing file lists did not fit inside the error window