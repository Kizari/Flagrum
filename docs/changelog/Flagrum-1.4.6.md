## Flagrum 1.4.6

_Released 5 April 2023_

### New Features

- 3D viewer can now preview LODs of models that have them
- Paths in Mod Manager now have a copy button next to them and won't copy with a line break or space
- Flagrum can now preview and export PS4 BTEX files and therefore also textured models
- Empty folders are now deleted when reverting applied mods

### Changes

- Mod card editor no longer loads the whole mod into memory, so editing details for larger mods is much quicker

### Bug Fixes

- Fixed bug where models without UV maps were crashing the 3D viewer
- Fixed bug where some file names were showing incorrectly due to the extension name being in the file name
- Fixed bug where mods could enter a bad state when editing conflicting mods—
  [#49](https://github.com/Kizari/Flagrum/issues/49)
- Fixed issue where thumbnails would rarely go missing—
  [#50](https://github.com/Kizari/Flagrum/issues/50)
- Fixed issue where individual prefabs would export incorrectly—
  [#51](https://github.com/Kizari/Flagrum/issues/51)
- Fixed bug where mods could not be deleted if one or more FFG files are missing—
  [#46](https://github.com/Kizari/Flagrum/issues/46)
- Fixed issue where Workshop mod thumbnails were not being updated after saving until restart